(function () {
    angular
        .module('app')
        .service('toast', function () {
            toastr.options.progressBar = true;
            toastr.options.timeOut = 4000;
            toastr.options.positionClass = "toast-bottom-right";
            return {

                /**
                 * Provides an informational message to the user
                 * @param {string} message The message to display
                 * @param {string} title The title to display
                 * @param {object} options The options for the toast message
                 */
                info: function (message, title, options) {
                    return toastr.info(message, title, options);
                },

                /**
                 * Provides a success message to the user
                 * @param {string} message The message to display
                 * @param {string} title The title to display
                 * @param {object} options The options for the toast message
                 */
                success: function (message, title, options) {
                    return toastr.success(message, title, options);
                },

                /**
                 * Provides a fail message to the user
                 * @param {string} message The message to display
                 * @param {string} title The title to display
                 * @param {object} options The options for the toast message
                 */
                fail: function (message, title, options) {
                    return toastr.error(message, title, options);
                },

                /**
                 * Provides a fail message to the user
                 * @param {string} message The message to display
                 * @param {string} title The title to display
                 * @param {object} options The options for the toast message
                 */
                error: function (message, title, options) {
                    return toastr.error(message, title, options);
                },

                /**
                 * Provides a warning to the user
                 * @param {string} message The message to display
                 * @param {string} title The title to display
                 * @param {object} options The options for the toast message
                 */
                warning: function (message, title, options) {
                    return toastr.warning(message, title, options);
                },

                onError: function (message) {
                    return function () {
                        return toastr.error(message);
                    };
                }
            };
        });
})();