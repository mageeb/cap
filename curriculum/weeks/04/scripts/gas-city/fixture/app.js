const canvas = document.querySelector('#canvas');
const ctx = canvas.getContext('2d');
let drawing = false;
function draw(e) {
  if (!drawing) return;
  const r = canvas.getBoundingClientRect();
  const x = (e.clientX-r.left)*canvas.width/r.width;
  const y = (e.clientY-r.top)*canvas.height/r.height;
  ctx.fillStyle = document.querySelector('#tool').value === 'eraser' ? '#ffffff' : document.querySelector('#color').value;
  ctx.beginPath(); ctx.arc(x,y,8,0,Math.PI*2); ctx.fill();
}
canvas.addEventListener('pointerdown', e => { drawing=true; canvas.setPointerCapture(e.pointerId); draw(e); });
canvas.addEventListener('pointermove', draw);
canvas.addEventListener('pointerup', () => { drawing=false; });
document.querySelector('#clear').addEventListener('click', () => ctx.clearRect(0,0,canvas.width,canvas.height));
