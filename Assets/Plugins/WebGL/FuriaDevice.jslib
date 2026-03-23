mergeInto(LibraryManager.library, {
    Furia_IsMobileBrowser: function () {
        var userAgent = navigator.userAgent || navigator.vendor || window.opera || "";
        var isMobileUserAgent = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(userAgent);
        var hasCoarsePointer = window.matchMedia && window.matchMedia("(pointer: coarse)").matches;
        var hasTouchPoints = (navigator.maxTouchPoints || 0) > 0;
        var isSmallScreen = Math.min(window.innerWidth || 0, window.innerHeight || 0) <= 1024;

        return (isMobileUserAgent && (hasCoarsePointer || hasTouchPoints || isSmallScreen)) ? 1 : 0;
    }
});
