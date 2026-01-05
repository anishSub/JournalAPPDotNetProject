window.editor = {
  insertText: function (elementId, prefix, suffix) {
    const textarea = document.getElementById(elementId);
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const text = textarea.value;
    const selectedText = text.substring(start, end);

    const before = text.substring(0, start);
    const after = text.substring(end);

    const newText = before + prefix + selectedText + suffix + after;

    textarea.value = newText;

    // Restore selection (including the inserted formatting)
    // Or place cursor inside?
    // If selection was empty, place cursor between tags: "**|**"
    // If selection existed, select the wrapped text: "**bold**"

    textarea.focus();
    if (start === end) {
      textarea.setSelectionRange(start + prefix.length, start + prefix.length);
    } else {
      textarea.setSelectionRange(start, end + prefix.length + suffix.length);
    }

    // Trigger input event to notify Blazor of the change
    const event = new Event('input', { bubbles: true });
    textarea.dispatchEvent(event);
  }
};
