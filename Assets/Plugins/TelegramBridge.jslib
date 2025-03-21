mergeInto(LibraryManager.library, {
  Telegram_GetInitData: function () {
    // Convertit la chaîne d'initData en mémoire allouée
    var initData = window.Telegram.WebApp.initData || "";
    return allocate(intArrayFromString(initData), "i8", ALLOC_NORMAL);
  },
});
