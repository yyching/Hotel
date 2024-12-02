// Dropdown
(function ($) {
    try {
        var menu = $('.js-item-menu');
        var subMenuIsShowed = -1;

        // Toggle Dropdown
        menu.on('click', function (e) {
            e.preventDefault();
            $('.js-right-sidebar').removeClass("show-sidebar");

            if (menu.index(this) === subMenuIsShowed) {
                $(this).toggleClass('show-dropdown');
                subMenuIsShowed = -1;
            } else {
                menu.removeClass("show-dropdown");
                $(this).addClass('show-dropdown');
                subMenuIsShowed = menu.index(this);
            }
        });

        // Prevent propagation on dropdown clicks
        $(".js-item-menu, .js-dropdown").click(function (event) {
            event.stopPropagation();
        });

        // Close dropdowns on body click
        $("body,html").on("click", function () {
            menu.removeClass("show-dropdown");
            subMenuIsShowed = -1;
        });

    } catch (error) {
        console.error("An error occurred in dropdown handling:", error);
    }
})(jQuery);

// Profile picture change functionality
const fileInput = document.getElementById('profile-pic-upload');
const profilePic = document.querySelector('.profile-pic');

fileInput.addEventListener('change', function (event) {
    const file = event.target.files[0];
    if (file) {
        const imageUrl = URL.createObjectURL(file);
        profilePic.src = imageUrl;
    }
});

//// Initiate GET request (AJAX-supported)
//$(document).on('click', '[data-get]', e => {
//    e.preventDefault();
//    const url = e.target.dataset.get;
//    location = url || location;
//});

$(document).on('click', '[data-get]', function (e) {
    e.preventDefault();
    const url = e.target.dataset.get;
    if (url) {
        location.href = url;
    } else {
        console.warn('No URL found in data-get attribute');
    }
});
