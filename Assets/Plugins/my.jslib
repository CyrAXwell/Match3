mergeInto(LibraryManager.library, {

  Hello: function () {
    window.alert("Hello, world!");
  },

  ShowFullscreenAdv: function () {
    ysdk.adv.showFullscreenAdv({
        callbacks: {
            onClose: function(wasShown) {

            },
            onError: function(error) {
              // some action on error
            }
        }
    })
  },

  UpdateLeaderboardScore: function (value) {
    ysdk.getLeaderboards()
          .then(lb => {
            lb.setLeaderboardScore('Level', value);
            console.log('setLeaderboardScore', value);
          });
  },

});