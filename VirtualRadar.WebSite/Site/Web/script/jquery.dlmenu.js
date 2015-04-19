/**
 * jquery.dlmenu.js v1.0.1
 * http://www.codrops.com
 *
 * Licensed under the MIT license.
 * http://www.opensource.org/licenses/mit-license.php
 *
 * Copyright 2013, Codrops
 * http://www.codrops.com
 *
 * Modified by AGW, mostly to remove transitions & animations and to support scrolling menus & positioning menus that
 * won't fit on screen.
 */
;( function( $, window, undefined ) {

    'use strict';

    $.DLMenu = function( options, element ) {
        this.$el = $( element );
        this._init( options );
    };

    // the options
    $.DLMenu.defaults = {
        // callback: click a link that has a sub menu
        // el is the link element (li); name is the level name
        onLevelClick : function( el, name ) { return false; },
        // callback: click a link that does not have a sub menu
        // el is the link element (li); ev is the event obj
        onLinkClick : function( el, ev ) { return false; },
        // text: shown in back links
        backLinkText: 'back'
    };

    $.DLMenu.prototype = {
        _init : function( options ) {

            // options
            this.options = $.extend( true, {}, $.DLMenu.defaults, options );
            // cache some elements and initialize some variables
            this._config();

            this._initEvents();

        },
        _config : function() {
            this.open = false;
            this.$trigger = this.$el.children( '.dl-trigger' );
            this.$menu = this.$el.children( 'ul.dl-menu' );
            this.$menuitems = this.$menu.find( 'li:not(.dl-back)' );
            this.$el.find( 'ul.dl-submenu' ).prepend($('<li/>').addClass('dl-back').append($('<a/>').attr('href', '#').text(this.options.backLinkText)));
            this.$back = this.$menu.find( 'li.dl-back' );
            this.originalPosition = this.$el.css(['top', 'right', 'bottom', 'left']);
        },
        _initEvents : function() {

            var self = this;

            this.$trigger.on( 'click.dlmenu', function() {

                if( self.open ) {
                    self._closeMenu();
                }
                else {
                    self._openMenu();
                }
                return false;

            } );

            this.$menuitems.on( 'click.dlmenu', function( event ) {

                event.stopPropagation();

                var $item = $(this),
                    $submenu = $item.children( 'ul.dl-submenu' );
                var disabled = $item.hasClass( 'dl-disabled' );

                if( !disabled && $submenu.length > 0 ) {

                    self.$menu.addClass( 'dl-subview' );
                    $item.addClass( 'dl-subviewopen' ).parents( '.dl-subviewopen:first' ).removeClass( 'dl-subviewopen' ).addClass( 'dl-subview' );

                    self.options.onLevelClick( $item, $item.children( 'a:first' ).text() );
                    self._ensureVisible();

                    return false;

                }
                else {
                    if( !disabled ) self.options.onLinkClick( $item, event );
                }

            } );

            this.$back.on( 'click.dlmenu', function( event ) {

                var $this = $( this ),
                    $submenu = $this.parents( 'ul.dl-submenu:first' ),
                    $item = $submenu.parent();

                $item.removeClass( 'dl-subviewopen' );

                var $subview = $this.parents( '.dl-subview:first' );
                if( $subview.is( 'li' ) ) {
                    $subview.addClass( 'dl-subviewopen' );
                }
                $subview.removeClass( 'dl-subview' );

                self._ensureVisible();

                return false;

            } );

        },
        _ensureVisible: function() {
            this.$el.css({
                height: '',
                'overflow-x': 'visible',
                'overflow-y': 'visible'
            });
            this.$el.css(this.originalPosition);

            var offset = this.$el.offset();
            if(offset.top < 0) this.$el.css({
                top: 0,
                bottom: 'auto'
            });
            if(offset.left < 0) this.$el.css({
                left: 0,
                right: 'auto'
            });
            offset = this.$el.offset();

            var view = $(window);
            var menuWidth = this.$el.outerWidth();
//            var menuHeight = this.$el.outerHeight();  // <-- doesn't work in Firefox, previous reset of height does nothing - tried '', null and 'auto', always returns previous menu's height by the time it gets to here and not current menu's height
            var menuHeight = this.$el[0].scrollHeight;  // <-- this does work in Firefox :)
            var availWidth = view.width() - offset.left;
            var availHeight = view.height() - offset.top;
            if(menuWidth > availWidth) this.$el.css('overflow-x', 'auto');
            if(menuHeight > availHeight) {
                this.$el.css({
                    height: availHeight,
                    'overflow-y': 'auto'
                });
            }
        },
        dispose: function() {
            this.$trigger.off();
            this.$menuitems.off();
            this.$back.off();
        },
        closeMenu : function() {
            if( this.open ) {
                this._closeMenu();
            }
        },
        _closeMenu : function() {
            this.$menu.removeClass( 'dl-menuopen' );
            this.$menu.addClass( 'dl-menu-toggle' );
            this.$trigger.removeClass( 'dl-active' );
            this._resetMenu();
            this.open = false;
        },
        openMenu : function() {
            if( !this.open ) {
                this._openMenu();
            }
        },
        _openMenu : function() {
            var self = this;
            this.$menu.addClass( 'dl-menuopen');
            this.$trigger.addClass( 'dl-active' );
            self._ensureVisible();
            this.open = true;
        },
        // resets the menu to its original state (first level of options)
        _resetMenu : function() {
            this.$menu.removeClass( 'dl-subview' );
            this.$menuitems.removeClass( 'dl-subview dl-subviewopen' );
        }
    };

    var logError = function( message ) {
        if ( window.console ) {
            window.console.error( message );
        }
    };

    $.fn.dlmenu = function( options ) {
        if ( typeof options === 'string' ) {
            var args = Array.prototype.slice.call( arguments, 1 );
            this.each(function() {
                var instance = $.data( this, 'dlmenu' );
                if ( !instance ) {
                    logError( "cannot call methods on dlmenu prior to initialization; " +
                    "attempted to call method '" + options + "'" );
                    return;
                }
                if ( !$.isFunction( instance[options] ) || options.charAt(0) === "_" ) {
                    logError( "no such method '" + options + "' for dlmenu instance" );
                    return;
                }
                instance[ options ].apply( instance, args );
            });
        }
        else {
            this.each(function() {
                var instance = $.data( this, 'dlmenu' );
                if ( instance ) {
                    instance._init();
                }
                else {
                    instance = $.data( this, 'dlmenu', new $.DLMenu( options, this ) );
                }
            });
        }
        return this;
    };

} )( jQuery, window );