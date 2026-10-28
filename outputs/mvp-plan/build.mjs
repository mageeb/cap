import fs from 'node:fs/promises';
import {Workbook, SpreadsheetFile} from 'file:///C:/Users/HanyG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const dir='C:/Repos/cap/outputs/mvp-plan';
const wb=Workbook.create();
const rows=[];
const add=(id,parent,type,title,check,deps,source,gate='',minutes='')=>rows.push([id,parent,type,title,check,deps,'Proposed',gate,minutes,source,'']);
add('MVP','','MVP','Translation review web MVP','One reviewer completes one small Arabic-to-English document, saves and resumes corrections, and indicates completion.','','W5 §6–8; W6 §17');
const groups=[
['1','Agree the bounded experiment','Approved scope, UX and measurable acceptance criteria.','', 'W5 §6–8, §12',[
 ['1','Resolve product limits','Agreed format, size, completion and failure rules are explicit.',[
 ['List format and document-size options','Record supported inputs, rejection cases and candidate size limits.'],
 ['Confirm input boundaries with product owner','Record explicit format and size decisions; leave unresolved choices open.'],
 ['Define translation trigger and recovery','Specify explicit or automatic start, failure display, retry and partial-result policy.'],
 ['Define completion and draft retention','Resolve paragraph/document completion and minimum original-draft/history behavior.']]],
 ['2','Define the acceptance experiment','Reviewed PRD and UX describe observable success.',[
 ['Write core workflow acceptance examples','Cover upload, draft, ordered review, correction, save, resume and completion.'],
 ['Annotate error and save states in wireframes','Include invalid upload, translation error and unsuccessful save.'],
 ['Agree pilot success measures','Record quality/usefulness and timing measures with owner-approved thresholds; examples are not targets.']]]]],
['2','Define architecture and contracts','Reviewed design supports the bounded workflow.','1','W6 §4–7, §12–15',[
 ['1','Select a minimal implementation design','Stack, storage and release/access assumptions have explicit rationale.',[
 ['Compare viable stack and database options','Compare workflow fit, durability, consistency, cost and maintainability; distinguish course defaults.'],
 ['Record selected architecture and access boundary','Document selected stack/storage and private-preview access model with alternatives.'],
 ['Define book and paragraph schema','Specify keys, ownership, source/draft/edited text, explicit position and chosen completion state.'],
 ['Define API and consistency contracts','Describe create/read/update payloads, errors, book ownership checks and read-after-save behavior.']]],
 ['2','Create a runnable baseline','Setup, checks and initial persistence can run from documented commands.',[
 ['Create minimal backend entry point','A smoke check reaches the backend using the selected runtime.'],
 ['Create minimal web entry point','Frontend starts and calls the backend smoke endpoint.'],
 ['Add initial schema migration','Apply selected schema to an empty test store and verify constraints.'],
 ['Document local run and test commands','A clean-environment walkthrough identifies configuration and secret inputs.']]]]],
['3','Upload and organize source','Valid bounded input becomes durably ordered paragraphs.','2','W5 §7; W6 §5, §7, §12',[
 ['1','Validate and create the document','Accepted input creates a book; invalid input is rejected.',[
 ['Implement agreed input validation predicate','Boundary tests cover empty, unsupported and oversized inputs.'],
 ['Implement document creation API','Valid fixture returns a stable book ID; invalid fixture creates no usable book.'],
 ['Wire upload form to API','Show accepted result and meaningful validation/server errors.']]],
 ['2','Persist paragraph identity and order','Source passages retain stable identity and intended sequence.',[
 ['Implement agreed paragraph segmentation','Representative Arabic fixture preserves source text and agreed paragraph boundaries.'],
 ['Persist source paragraphs and positions','Saved fixture has stable book/paragraph IDs and explicit unique positions.'],
 ['Return paragraphs in explicit order','Read test returns reading order independently of generated IDs.']]]]],
['4','Generate a machine draft','External translation produces preserved aligned draft text.','3','W5 §7; W6 §7, §13',[
 ['1','Connect translation provider','A configured external provider maps output to the correct source passage.',[
 ['Define provider adapter and deterministic fake','Success, timeout and malformed-response fixtures exercise one adapter contract.'],
 ['Implement external provider request','A bounded permitted sample returns target text; credentials remain server-side.'],
 ['Validate and persist draft alignment','Reject missing/mismatched results and persist successful draft against stable paragraph IDs.']]],
 ['2','Handle translation failure safely','Reviewer sees progress/failure and can recover without losing work.',[
 ['Expose translation state in API','Agreed pending/success/failure states are distinguishable and readable.'],
 ['Display translation state and recovery action','UI exposes the agreed start/retry interaction and useful failure message.'],
 ['Implement safe repeat-request rule','A repeated request follows agreed policy without overwriting reviewer edits.'],
 ['Verify provider failures and partial results','Timeout and partial-failure fixtures preserve existing successful drafts/corrections.']]]]],
['5','Review aligned paragraphs','Reviewer can compare and edit passages in reading order.','4','W5 §3, §7; W6 §2, §12',[
 ['1','Read source beside target','Arabic source and English target remain correctly paired.',[
 ['Render one aligned paragraph pair','Check Arabic RTL and English LTR text with mixed punctuation and numbers.'],
 ['Add ordered paragraph navigation','First/last and previous/next behavior stays paired with the correct source.'],
 ['Handle unavailable draft state','Missing or failed translation cannot appear as a completed translation.']]],
 ['2','Correct target text in place','Editing changes the reviewed target while preserving agreed draft behavior.',[
 ['Bind editable target to paragraph identity','Typing and navigation do not apply edits to a different paragraph.'],
 ['Preserve original draft per agreed contract','Correction updates reviewed text; original output follows the approved retention rule.'],
 ['Test Unicode editing and navigation','Arabic/English symbols, line breaks and paragraph switching preserve intended text.']]]]],
['6','Save, resume and complete review','Reviewer can stop, return to durable corrections and indicate completion.','5','W5 §7; W6 §6–7, §13',[
 ['1','Save durable corrections','Successful save is visible and latest saved content is readable.',[
 ['Implement paragraph update persistence','A saved edit survives a fresh storage connection and subsequent read.'],
 ['Enforce operation and book access','Unauthorized operation and wrong-book requests are rejected; permitted reviewer can save.'],
 ['Wire save interaction and dirty state','UI distinguishes unsaved, saving, saved and failed without claiming premature success.'],
 ['Test unsuccessful save recovery','Failed/interrupted save preserves visible edit and supports agreed retry behavior.']]],
 ['2','Resume and indicate completion','Reload restores work and completion follows the agreed rule.',[
 ['Load saved document and review position','Reopen restores latest saved content and agreed resume location/behavior.'],
 ['Implement agreed completion state','Persist chosen granularity and transition rules, including edits after completion.'],
 ['Expose completion control and feedback','UI accurately reflects persisted state without adding unapproved export gating.'],
 ['Test reload and completion boundaries','Reload, incomplete material and post-completion edits follow the approved acceptance examples.']]]]],
['7','Validate the MVP experiment','Evidence demonstrates workflow and informs the next iteration.','6','W5 §8; W6 §3, §17; proposed implementation verification',[
 ['1','Demonstrate the integrated workflow','End-to-end and recovery evidence is recorded.',[
 ['Run bounded workflow smoke scenario','Upload approved fixture, translate, correct, save, reload and complete with recorded results.'],
 ['Run integrated failure scenarios','Record invalid input, provider failure, failed save, wrong-book access and alignment checks.'],
 ['Verify restart durability','Restart replaceable application process; saved source, draft and corrections remain accessible.']]],
 ['2','Collect reviewer feedback and hand off','Pilot findings are compared with agreed success measures.',[
 ['Prepare bounded pilot checklist','Record approved fixture, participant, measures and known limitations.'],
 ['Capture one pilot observation segment','Record observed friction and outcomes; repeat bounded segments as needed for the agreed document.'],
 ['Summarize evidence and next actions','Compare actual pilot measures to agreed criteria; distinguish defects from later enhancements.']]]]]
];
for(const [g,title,check,dep,source,stories] of groups){
 add(g,'MVP','Epic',title,check,dep,source,g==='1'?'Product choices unresolved':g==='2'?'Architecture choices unresolved':'Depends on approved upstream contracts');
 let prev=dep;
 for(const [s,st,sc,tasks] of stories){
  const sid=`${g}.${s}`;add(sid,g,'Story',st,sc,prev,source);
  let pd=prev;
  tasks.forEach(([tt,tc],i)=>{const id=`${sid}.${i+1}`;add(id,sid,'Task',tt,tc,pd,source,'',5);pd=id;});prev=pd;
 }
}
const ws=wb.worksheets.add('MVP hierarchy');
ws.getRange('A2').values=[['MVP implementation hierarchy']];
ws.getRange('A3').values=[['Proposed plan from Weeks 5–6. All items are unstarted. Resolve product/design gates before dependent implementation.']];
const headers=['ID','Parent ID','Type','Work item','Completion check','Depends on ID','Status','Gate / constraint','Slice target (min)','Source key','Verification evidence'];
ws.getRange(`A5:K${rows.length+5}`).values=[headers,...rows];
ws.tables.add(`A5:K${rows.length+5}`,true,'MvpWorkItems');
ws.getRange(`A5:K${rows.length+5}`).format.font={name:'Arial',size:11,color:'#172B4D'};
ws.getRange('A5:K5').format={fill:'#243B53',font:{name:'Arial',size:11,bold:true,color:'#FFFFFF'},rowHeight:32};
const widths=[90,90,90,380,620,120,100,250,130,260,280];
widths.forEach((w,i)=>ws.getRange(`${String.fromCharCode(65+i)}5:${String.fromCharCode(65+i)}${rows.length+5}`).format.columnWidth=w/7);
ws.getRange(`A6:K${rows.length+5}`).format.rowHeight=60;
ws.getRange(`D6:K${rows.length+5}`).format.wrapText=true;
rows.forEach((r,i)=>{if(r[2]!=='Task'){ws.getRange(`A${i+6}:K${i+6}`).format.fill=r[2]==='Epic'?'#D9E5F2':'#EDF2F7';ws.getRange(`D${i+6}`).format.font.bold=true;}});
ws.getRange('A2').format.font={name:'Arial',size:16,bold:true};
ws.freezePanes.freezeRows(5);
const guide=wb.worksheets.add('Scope and use');
const notes=[
 ['Topic','Plan guidance','Source / reference'],
 ['Purpose','Proposed implementation plan for the translation-review case study, not a claim that Week 6 adopted an execution backlog.','W6 §15, §17'],
 ['Hierarchy','MVP > Epic > Story > Task. IDs and Parent ID retain hierarchy when filtered. Dependencies reference completion of the identified item.','DEC-031'],
 ['Execution','Tasks are provisional 5-minute implementation/check slices. Split further when a selected stack exposes more work. Parent items are not 5-minute tasks. No project duration is estimated.','Repository AGENTS.md; DEC-031'],
 ['Tracking','Status and Verification evidence are editable. Proposed means unstarted; record actual evidence before marking complete. No dates or owners are assumed.','DEC-031'],
 ['Scope','One reviewer, one small Arabic-to-English book/article, web delivery, paragraph-based review.','W5 §6–7'],
 ['Product gate','Decide input format and size, translation trigger/retry, completion granularity, minimum draft/history behavior and measurable success criteria.','W5 §7–8, §12'],
 ['Architecture gate','Choose and defend stack/storage, schema, routes, consistency and access model. Course overview describes a production learning path; Weeks 5–6 do not establish a selected case-study stack.','W5 §9; W6 §4–7, §12–15; course overview'],
 ['Release boundary','Determine private-preview access before implementation. Verify backend operation and book-level access before exposing the application. Deployment is not performed by this plan.','W6 §3, §6'],
 ['Later scope','Multiple reviewers/roles, adjudication, many-book management, publishing, fine-tuning corpus, offline synchronization and custom distributed scaling.','W5 §2, §4, §6; W6 §8–11, §16'],
 ['Not automatically required','PDF/OCR, export/download gating, scripture lookup, terminology assistance, TTS and rich diff/history remain unapproved additions. Reassess after the core pilot.','W5 §3, §7; W6 §2'],
 ['Success','Agree pilot measures before coding. The 30-page/30-minute example and reported throughput are not adopted targets or measured MVP results.','W5 §3, §8'],
 ['Decision record','DEC-031 remains Pending user review. This workbook proposes sequencing and decomposition without selecting unresolved product or technology choices.','../../DECISIONS.md#dec-031-mvp-work-item-hierarchy'],
 ['W5','Organized Week 5 source: product, workflow, exclusions and open questions.','../../curriculum/weeks/05/session-notes.md'],
 ['W6','Organized Week 6 source: architecture, contracts, storage, consistency and unresolved choices.','../../curriculum/weeks/06/session-notes.md'],
 ['Week 5 assignment','PRD and UX deliverables provide design inputs, not completed implementation.','../../curriculum/weeks/05/homework.md'],
 ['Week 6 assignment','Database comparison/choice, schema and API contracts precede service internals.','../../curriculum/weeks/06/homework.md'],
 ['Course overview','Program-level requirements; reconcile with case-study choices at architecture gate.','../../CAP 2.0 AI-Native.md'],
 ['Delivery plan','Runnable baseline and access checks precede later deployment work.','../../conductor/plan.md']
];
guide.getRange(`A5:C${notes.length+4}`).values=notes;
guide.getRange('A2').values=[['Scope and use']];
guide.getRange(`A5:C${notes.length+4}`).format.font={name:'Arial',size:11,color:'#172B4D'};
guide.getRange('A2').format.font={name:'Arial',size:16,bold:true};
guide.getRange('A5:C5').format={fill:'#243B53',font:{name:'Arial',size:11,bold:true,color:'#FFFFFF'}};
for(const [col,w] of [['A',170],['B',740],['C',440]])guide.getRange(`${col}5:${col}${notes.length+4}`).format.columnWidth=w/7;
guide.getRange(`A5:C${notes.length+4}`).format.wrapText=true;
guide.getRange(`A6:C${notes.length+4}`).format.rowHeight=65;
const ids=new Set(rows.map(r=>r[0]));
if(ids.size!==rows.length)throw Error('Duplicate IDs');
for(const r of rows){if(r[1]&&!ids.has(r[1]))throw Error('Invalid parent');if(r[5]&&!ids.has(r[5]))throw Error('Invalid dependency');}
wb.recalculate();
console.log((await wb.inspect({kind:'table',range:'MVP hierarchy!A5:G10',tableMaxRows:6,tableMaxCols:7,include:'values'})).ndjson);
await fs.mkdir(dir,{recursive:true});
for(const [sheet,range,name] of [['MVP hierarchy','A5:G14','hierarchy-preview'],['Scope and use','A5:C12','scope-preview']]){
 const image=await wb.render({sheetName:sheet,range,scale:1,format:'png'});await fs.writeFile(`${dir}/${name}.png`,new Uint8Array(await image.arrayBuffer()));
}
await (await SpreadsheetFile.exportXlsx(wb)).save(`${dir}/mvp-implementation-plan.xlsx`);
console.log(JSON.stringify({rows:rows.length,tasks:rows.filter(r=>r[2]==='Task').length,epics:groups.length,checks:'unique IDs, valid parents and dependencies, sources and completion checks populated'}));
