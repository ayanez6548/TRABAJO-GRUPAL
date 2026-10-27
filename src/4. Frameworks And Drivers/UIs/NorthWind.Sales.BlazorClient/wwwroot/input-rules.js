(() => {
  const previous = new WeakMap();
  const patterns = {
    text: /^[\s\S]*$/,
    letters: /^[\p{L}\p{M} ]*$/u,
    digits: /^[0-9]*$/,
    code: /^[a-zA-Z0-9]*$/,
    money: /^[0-9]{0,6}([.,][0-9]{0,2})?$/,
    address: /^[\p{L}\p{M}\p{N} #.,/\-]*$/u
  };
  const controlled = target => target instanceof HTMLInputElement && target.dataset.inputRule;
  const valid = (input, value) => {
    if (input.maxLength >= 0 && value.length > input.maxLength) return false;
    if (input.dataset.inputRule === 'email') {
      if (![...value].every(character => input.dataset.emailCharacters?.includes(character))) return false;
      if (value.split('@').length > 2) return false;
    } else if (!patterns[input.dataset.inputRule]?.test(value)) return false;
    if (input.dataset.inputRule === 'digits' && value) {
      const number = Number(value);
      if (input.dataset.minimum && number < Number(input.dataset.minimum)) return false;
      if (input.dataset.maximum && number > Number(input.dataset.maximum)) return false;
    }
    return true;
  };
  const candidate = (input, text) => input.value.slice(0, input.selectionStart ?? input.value.length)
    + text + input.value.slice(input.selectionEnd ?? input.value.length);
  // Email inputs do not expose a selection range. Validate inserted characters here;
  // the input handler checks the resulting value before Blazor receives the event.
  const insertionValid = (input, text) => input.selectionStart === null && input.dataset.inputRule === 'email'
    ? (input.maxLength < 0 || text.length <= input.maxLength)
      && [...text].every(character => input.dataset.emailCharacters?.includes(character))
    : valid(input, candidate(input, text));
  const reject = (input, event) => {
    event.preventDefault();
    const message = input.dataset.inputMessage || 'Revisa los caracteres permitidos y el límite de este campo.';
    window.northwindWarnings.show('Revisa los datos ingresados', [message]);
  };
  document.addEventListener('focusin', event => {
    if (controlled(event.target)) previous.set(event.target, event.target.value);
  }, true);
  document.addEventListener('beforeinput', event => {
    const input = event.target;
    if (!controlled(input) || event.isComposing || event.data == null) return;
    if (!insertionValid(input, event.data)) reject(input, event);
  }, true);
  document.addEventListener('paste', event => {
    const input = event.target;
    if (controlled(input) && !insertionValid(input, event.clipboardData.getData('text'))) reject(input, event);
  }, true);
  document.addEventListener('input', event => {
    const input = event.target;
    if (!controlled(input)) return;
    if (event.isComposing) { event.stopImmediatePropagation(); return; }
    if (!valid(input, input.value)) {
      input.value = previous.get(input) ?? '';
      event.stopImmediatePropagation();
      reject(input, event);
    } else {
      previous.set(input, input.value);
    }
  }, true);
  document.addEventListener('compositionend', event => {
    if (controlled(event.target)) event.target.dispatchEvent(new Event('input', {bubbles: true}));
  }, true);
})();
