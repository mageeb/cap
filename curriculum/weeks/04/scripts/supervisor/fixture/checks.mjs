import assert from 'node:assert/strict';
import { loadPreferences, savePreferences } from './app/settings.js';
import { renderToolbar } from './app/toolbar.js';
const defaults = {tool:'pencil',color:'#152536'}, key = 'cap.paint.preferences.v1';
function storage(raw = null) {
  return {getItem: k => {assert.equal(k,key); return raw;},
    setItem: (k,v) => {assert.equal(k,key); raw = v;}};
}
assert.deepEqual(loadPreferences(storage()),defaults);
for (const raw of ['{broken','null','[]','{"tool":"brush","color":"#188D91"}',
  '{"tool":"eraser","color":"purple"}']) assert.deepEqual(loadPreferences(storage(raw)),defaults);
const s = storage();
savePreferences(s,{tool:'pencil',color:'#188D91'});
assert.deepEqual(loadPreferences(s),{tool:'pencil',color:'#188D91'});
savePreferences(s,{tool:'eraser',color:'#188D91'});
assert.deepEqual(loadPreferences(s),{tool:'eraser',color:'#188D91'});
savePreferences(s,{tool:'bad',color:'bad'});
assert.deepEqual(loadPreferences(s),defaults);
const denied = {getItem(){throw Error('denied');},setItem(){throw Error('denied');}};
assert.deepEqual(loadPreferences(denied),defaults);
assert.doesNotThrow(() => savePreferences(denied,defaults));
const html = renderToolbar({tool:'eraser',color:'#188D91'});
for (const [id,value] of [['tool','eraser'],['color','#188D91']]) {
  const select = html.match(new RegExp(`<select\\b[^>]*id=["']${id}["'][^>]*>([\\s\\S]*?)</select>`,'i'));
  assert.ok(select,`Missing select#${id}`);
  const options = select[1].match(/<option\b[^>]*>/gi) || [];
  assert.ok(options.some(o => new RegExp(`value=["']${value}["']`).test(o) && /\bselected\b/i.test(o)),
    `Selected ${id} must be ${value}`);
}
console.log('PASS: module defaults, invalid data, pencil/teal, eraser/teal, denied storage, selected controls.');
