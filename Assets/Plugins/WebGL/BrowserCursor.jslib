mergeInto(LibraryManager.library, {
  SetBrowserCursor: function(cursorStyle) {
    if (Module.canvas) {
      Module.canvas.style.cursor = UTF8ToString(cursorStyle);
    }
  }
});
