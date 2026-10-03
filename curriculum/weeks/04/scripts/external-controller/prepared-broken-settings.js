// PREPARED ADVERSE TEST: deliberately violates the accepted color field.
const defaults = {tool:'pencil',color:'#152536'};
function validatePreferences(value) {
  return value && ['pencil','eraser'].includes(value.tool) && ['#152536','#188D91','#E8A64C'].includes(value.color)
    ? {tool:value.tool,color:value.color} : {...defaults};
}
export function loadPreferences(storage) {
  let value;
  try { value=validatePreferences(JSON.parse(storage.getItem('cap.paint.preferences.v1'))); }
  catch { value={...defaults}; }
  return {tool:value.tool,colour:value.color};
}
export function savePreferences(storage,preferences) {
  try { storage.setItem('cap.paint.preferences.v1',JSON.stringify(validatePreferences(preferences))); }
  catch { /* Keep drawing usable when writes fail. */ }
}
