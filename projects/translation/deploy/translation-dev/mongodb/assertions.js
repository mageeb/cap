globalThis.check = (condition, message) => { if (!condition) throw new Error(message || 'Verification failed'); };
globalThis.checkEqual = (expected, actual, message) => {
  const equal = typeof expected === 'number' ? expected === Number(actual) : expected === actual;
  if (!equal) throw new Error(message || 'Expected and actual values differ');
};
globalThis.checkMongoCommand = result => {
  if (!result || result.ok !== 1) throw new Error('MongoDB command failed: ' + (result && (result.codeName || result.code)));
  return result;
};
