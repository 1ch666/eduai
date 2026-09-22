mergeInto(LibraryManager.library, {
  CourtTouchChoices: function(a, b, c, d) {
    if (window.courtTouchChoices) window.courtTouchChoices([UTF8ToString(a), UTF8ToString(b), UTF8ToString(c), UTF8ToString(d)]);
  },
  CourtTouchClose: function() {
    if (window.courtTouchChoices) window.courtTouchChoices([]);
  },
  CourtTouchMessage: function(text, seconds) {
    if (window.courtTouchMessage) window.courtTouchMessage(UTF8ToString(text), seconds);
  }
});
