window.northwindConfirmation = {
  open: (id, reference) => {
    const dialog = document.getElementById(id);
    if (dialog && !dialog.open) {
      dialog.addEventListener('cancel', event => { event.preventDefault(); reference.invokeMethodAsync('Cancel'); });
      dialog.showModal();
      window.northwindWarnings.bringToFront();
      document.documentElement.classList.add('modal-open');
    }
  },
  close: id => { const dialog = document.getElementById(id); if (dialog?.open) dialog.close(); document.documentElement.classList.toggle('modal-open', !!document.querySelector('dialog[open]')); }
};
window.northwindInvoice = {
  createPdfUrl: base64 => URL.createObjectURL(new Blob([Uint8Array.from(atob(base64), c => c.charCodeAt(0))], {type: 'application/pdf'})),
  openDialog: (id, reference) => {
    const dialog = document.getElementById(id);
    if (dialog && !dialog.open) {
      dialog.addEventListener('cancel', event => { event.preventDefault(); reference.invokeMethodAsync('ClosePreview'); });
      dialog.showModal();
      window.northwindWarnings.bringToFront();
      document.documentElement.classList.add('modal-open');
    }
  },
  closeDialog: id => { const dialog = document.getElementById(id); if (dialog?.open) dialog.close(); document.documentElement.classList.toggle('modal-open', !!document.querySelector('dialog[open]')); },
  print: id => { const frame = document.getElementById(id); if (frame) { frame.contentWindow.focus(); frame.contentWindow.print(); } }
};
