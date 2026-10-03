import { renderToolbar } from './toolbar.js';
const preferences = {tool:'pencil',color:'#152536'};
document.querySelector('#toolbar').innerHTML = renderToolbar(preferences);
const canvas = document.querySelector('#paint'), ctx = canvas.getContext('2d');
let drawing = false;
function position(e) {
  const b = canvas.getBoundingClientRect();
  return [(e.clientX-b.left)*canvas.width/b.width,(e.clientY-b.top)*canvas.height/b.height];
}
canvas.addEventListener('pointerdown', e => {
  drawing = true; canvas.setPointerCapture(e.pointerId);
  ctx.beginPath(); ctx.moveTo(...position(e));
});
canvas.addEventListener('pointermove', e => {
  if (!drawing) return;
  ctx.strokeStyle = preferences.color; ctx.lineWidth = 5; ctx.lineCap = 'round';
  ctx.lineTo(...position(e)); ctx.stroke();
});
canvas.addEventListener('pointerup', () => drawing = false);
canvas.addEventListener('pointercancel', () => drawing = false);
