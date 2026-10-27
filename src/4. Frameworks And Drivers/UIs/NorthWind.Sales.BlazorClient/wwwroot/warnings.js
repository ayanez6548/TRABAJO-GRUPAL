(() => {
  const dialog = document.getElementById('northwind-warning');
  const title = document.getElementById('northwind-warning-title');
  const list = document.getElementById('northwind-warning-messages');
  const acknowledge = document.getElementById('northwind-warning-acknowledge');
  let returnFocus;
  const close = () => dialog.close();
  const updateScrollLock = () => document.documentElement.classList.toggle('modal-open', !!document.querySelector('dialog[open]'));
  const bringToFront = () => {
    if (!dialog.open) return;
    dialog.close();
    dialog.showModal();
    acknowledge.focus({preventScroll: true});
    updateScrollLock();
  };
  acknowledge.addEventListener('click', close);
  dialog.addEventListener('close', () => {
    updateScrollLock();
    if (!dialog.open && returnFocus?.isConnected) returnFocus.focus({preventScroll: true});
  });
  window.northwindWarnings = {
    show: (heading, messages) => {
      const existing = dialog.open ? [...list.children].map(item => item.textContent) : [];
      const unique = [...new Set([...existing, ...messages].map(message => String(message).trim()).filter(Boolean))];
      if (!unique.length) return;
      title.textContent = heading;
      list.replaceChildren(...unique.map(message => {
        const item = document.createElement('li');
        item.textContent = message;
        return item;
      }));
      if (dialog.open) bringToFront();
      else {
        returnFocus = document.activeElement;
        dialog.showModal();
        acknowledge.focus({preventScroll: true});
      }
      updateScrollLock();
    },
    bringToFront
  };
})();
