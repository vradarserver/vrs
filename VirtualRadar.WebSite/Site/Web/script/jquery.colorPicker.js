/***
 @title:
 Colour Picker

 @version:
 2.0

 @author:
 Andreas Lagerkvist

 @date:
 2008-09-16

 @url:
 http://andreaslagerkvist.com/jquery/colour-picker/

 @license:
 http://creativecommons.org/licenses/by/3.0/

 @copyright:
 2008 Andreas Lagerkvist (andreaslagerkvist.com)

 @requires:
 jquery, jquery.colourPicker.css, jquery.colourPicker.gif
 ***/
jQuery.fn.colourPicker = function (conf) {
    // Config for plug
    var config = jQuery.extend({
        id:			'jquery-colour-picker',	// id of colour-picker container
        ico:		'ico.gif',				// SRC to colour-picker icon  //AGW - this no longer has any effect
        title:		'Pick a colour',		// Default dialogue title
        inputBG:	true,					// Whether to change the input's background to the selected colour's
        speed:		500,					// Speed of dialogue-animation
        openTxt:	'Open colour picker',
        colours:    null                    // AGW: The colours to use
    }, conf);

    // Inverts a hex-colour
    var hexInvert = function (hex) {
        if(!hex || (hex.length !== 3 && hex.length !== 6)) return '000000';

        if(hex.length == 3) hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2]
        var r = parseInt(hex.substr(0, 2), 16);
        var g = parseInt(hex.substr(2, 2), 16);
        var b = parseInt(hex.substr(4, 2), 16);

        return (0.212671 * r + 0.715160 * g + 0.072169 * b) < 128 ? 'ffffff' : '000000'
    };

    var paintInputBackground = function(input) {
        var hex = input.val();
        if(hex) {
            var hash = hex[0] !== '#' ? hex = '#' + hex : hex;
            var noHash = hex.substr(1);
            input.css({background: hash, color: '#' + hexInvert(noHash)});
        }
    };

    var closeColourPicker = function() {
        colourPickerOpen = false;
        jQuery('a', colourPicker).off();
        colourPicker.hide(config.speed).appendTo(document.body);
    }

    // Add the colour-picker dialogue if not added
    var colourPicker = jQuery('#' + config.id);
    var colourPickerOpen = false;

    if (!colourPicker.length) {
        colourPicker = jQuery('<div id="' + config.id + '"></div>').appendTo(document.body).hide();

        // Remove the colour-picker if you click outside it (on body)
        jQuery(document.body).click(function(event) {
            if (!(jQuery(event.target).is('#' + config.id) || jQuery(event.target).parents('#' + config.id).length)) {
                closeColourPicker();
            }
        });
    }

    // For every select passed to the plug-in
    return this.each(function () {
        // Insert icon and input
        var input	= jQuery(this);  // AGW
        var icon = jQuery('<span/>').addClass('vrsIcon vrsIconButton vrsIcon-paint-format vrsContent ui-corner-all').insertAfter(input); // AGW
        var loc		= '';

        // AGW: Build a list of colours based on the colours in the select
        $.each(config.colours, function(idx, hex) {
            var title = '#' + hex;

            loc += '<li><a href="#" title="'
                + title
                + '" rel="'
                + hex
                + '" style="background: #'
                + hex
                + '; colour: '
                + hexInvert(hex)
                + ';">'
                + title
                + '</a></li>';
        });

        // If user wants to, change the input's BG to reflect the newly selected colour
        if (config.inputBG) {
            input.change($.proxy(function() { paintInputBackground(input); }));
            paintInputBackground(input);
        }

        // When you click the icon
        icon.click(function () {
            if(colourPickerOpen) closeColourPicker();
            else {
                colourPickerOpen = true;

                // Show the colour-picker below the input and fill it with colours
                var parent = icon.offsetParent();
                var position = input.position();//AGWicon.offset();
                position.top += input.outerHeight(true);
                var heading	= config.title ? '<h2>' + config.title + '</h2>' : '';

                colourPicker.html(heading + '<ul>' + loc + '</ul>').css({
                    position: 'absolute',
                    left: position.left + 'px',
                    top: position.top + 'px'
                }).appendTo(icon.offsetParent())
                    .show(config.speed);

                // When you click a colour in the colour-picker
                jQuery('a', colourPicker).click(function () {
                    // The hex is stored in the link's rel-attribute
                    var hex = jQuery(this).attr('rel');

                    input.val('#' + hex);
                    input.change();

                    // Hide the colour-picker and return false
                    closeColourPicker();

                    return false;
                });
            }

            return false;
        });
    });
};
