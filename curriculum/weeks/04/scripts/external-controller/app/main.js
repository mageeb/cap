const canvas = document.querySelector('#paint');
const context = canvas.getContext('2d');
let drawing = false;
function point(event) {
  const rect = canvas.getBoundingClientRect();
  return [(event.clientX - rect.left) * canvas.width / rect.width,
          (event.clientY - rect.top) * canvas.height / rect.height];
}
canvas.addEventListener('pointerdown', event => {
  drawing = true;
  canvas.setPointerCapture(event.pointerId);
  context.beginPath();
  context.moveTo(...point(event));
});
canvas.addEventListener('pointermove', event => {
  if (!drawing) return;
  context.strokeStyle = '#152536';
  context.lineWidth = 4;
  context.lineTo(...point(event));
  context.stroke();
});
canvas.addEventListener('pointerup', () => { drawing = false; });
canvas.addEventListener('pointercancel', () => { drawing = false; });
