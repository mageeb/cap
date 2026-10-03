# Human-owned gates for Demo 03. Workers never edit this file.
# Usage: python3 checks.py CRITERION APP_PATH [REVIEW_REPORT]
# Schema/UI/storage checks inspect the candidate's actual output. Integration
# and review also launch a local server and verify behavior in Chromium.

from pathlib import Path
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
import json, subprocess, sys, threading
ROOT = Path(__file__).resolve().parent
criterion, app = sys.argv[1], Path(sys.argv[2]).resolve()
assert criterion in {'schema','ui','storage','integration','review'}, 'Unknown criterion'

# Run a module check in a separate Node process with a short timeout.
def node(script):
    result = subprocess.run(['node', '--input-type=module', '-e', script], cwd=ROOT,
                            capture_output=True, text=True, timeout=30)
    if result.returncode:
        raise AssertionError(result.stderr or result.stdout)
    return result.stdout

prefix = "import assert from 'node:assert/strict';\n"
uri = lambda name: (app / name).as_uri()
# Gate 1: all Workers share the exact same tool/color contract.
if criterion == 'schema':
    assert json.loads((app / 'schema.json').read_text()) == {
        'tools': ['pencil', 'eraser'], 'colors': ['#152536','#188D91','#E8A64C'], 'default': {'tool': 'pencil', 'color': '#152536'}}
# Gate 2: the toolbar renders the selected options using the agreed IDs.
if criterion == 'ui':
    node(prefix + f"import {{renderToolbar}} from {json.dumps(uri('toolbar.js'))};\n" + r"""
const html=renderToolbar({tool:'eraser',color:'#188D91'});
for (const [id,value] of [['tool','eraser'],['color','#188D91']]) {
  const select=html.match(new RegExp(`<select\\b[^>]*id=["']${id}["'][^>]*>([\\s\\S]*?)</select>`,'i'));
  assert.ok(select,`Missing select#${id}`);
  const options=select[1].match(/<option\b[^>]*>/gi)||[];
  assert.ok(options.some(o => new RegExp(`value=["']${value}["']`).test(o) && /\bselected\b/i.test(o)));
}
""")
# Gate 3: storage preserves the interface and recovers from bad/denied data.
if criterion in ('storage', 'integration', 'review'):
    node(prefix + f"import {{loadPreferences,savePreferences}} from {json.dumps(uri('settings.js'))};\n" + r"""
const data = new Map(), storage = {getItem:k=>data.get(k)??null,setItem:(k,v)=>data.set(k,v)};
const defaults={tool:'pencil',color:'#152536'}, good={tool:'eraser',color:'#188D91'};
assert.deepEqual(loadPreferences(storage),defaults);
savePreferences(storage,good); assert.deepEqual(loadPreferences(storage),good);
assert.deepEqual(JSON.parse(data.get('cap.paint.preferences.v1')),good);
for (const bad of ['{broken',JSON.stringify({tool:'laser',color:'#188D91'}),JSON.stringify({tool:'pencil',colour:'#188D91'}),JSON.stringify({tool:'pencil',color:'blue'})]) {
  storage.setItem('cap.paint.preferences.v1',bad); assert.deepEqual(loadPreferences(storage),defaults);
}
const denied={getItem(){throw Error('denied');},setItem(){throw Error('denied');}};
assert.deepEqual(loadPreferences(denied),defaults);
assert.doesNotThrow(()=>savePreferences(denied,defaults));
savePreferences(storage,{tool:'bad',color:'bad'}); assert.deepEqual(loadPreferences(storage),defaults);
""")
# Gates 4/5: combine the real modules, then exercise actual browser behavior.
if criterion in ('integration', 'review'):
    node(prefix + f"import {{loadPreferences,savePreferences}} from {json.dumps(uri('settings.js'))};\n"
         + f"import {{renderToolbar}} from {json.dumps(uri('toolbar.js'))};\n" + r"""
const data=new Map(), storage={getItem:k=>data.get(k)??null,setItem:(k,v)=>data.set(k,v)};
const good={tool:'pencil',color:'#188D91'};
savePreferences(storage,good); assert.deepEqual(loadPreferences(storage),good);
const html=renderToolbar(loadPreferences(storage)); assert.ok(html.includes('#188D91'));
""")
    server = ThreadingHTTPServer(('127.0.0.1', 0), partial(SimpleHTTPRequestHandler, directory=str(app)))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    try:
        result = subprocess.run(['node', str(ROOT/'browser-check.mjs'),
            f'http://127.0.0.1:{server.server_port}', str(app.parent/'screenshots')],
            capture_output=True, text=True, timeout=60)
        assert result.returncode == 0, result.stderr or result.stdout
        print(result.stdout.strip())
    finally:
        server.shutdown(); server.server_close()
# A passing model report is required in addition to the independent checks.
if criterion == 'review':
    report = json.loads(Path(sys.argv[3]).read_text())
    assert report['verdict'] == 'pass' and report['gaps'] == []
    assert len(report['criteria']) == 3
    assert {item['id'] for item in report['criteria']} == {'reload','drawing','invalid_state'}
    assert all(item['status'] == 'met' for item in report['criteria'])
print(json.dumps({'criterion':criterion,'status':'passed'}))
