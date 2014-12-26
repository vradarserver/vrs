/*!
 * jQuery Mobile onPage
 * http://uglymongrel.com.com
 *
 * Copyright 2014 Alexander Schmitz and other contributors
 * Released under the MIT license.
 * http://jquery.org/license
 *
 * http://api.uglymongrel.com.com/jquery-mobile-onpage/
 */
//>>excludeStart("jqmBuildExclude", pragmas.jqmBuildExclude);
//>>description: A utility for easy binding to page events which uses same api as old page events
//>>label: onPage / offPage
//>>group: Utilities
//>>excludeEnd("jqmBuildExclude");
(function( factory ) {
	if ( typeof define === "function" && define.amd ) {

	// AMD. Register as an anonymous module.
	define([
		"jquery",
	], factory );
  } else {

	// Browser globals
	factory( jQuery );
  }
}(function( $ ) {
	var globalPagecontainer,
		events = {},
		from = [
			"pagecontainerbeforechange",
			"pagecontainerbeforeload",
			"pagecontainerbeforehide",
			"pagecontainerhide",
			"pagecontainerremove"
		],
		deffered = $.Deferred(function( defer ){
			$.widget( "mobile.pagecontainer", $.mobile.pagecontainer, {
				_create: function(){
					globalPagecontainer = this.element;
					defer.resolve();
					this._super();
				}
			});
		}).promise();

	$.fn.onPage = function( eventName, element, callback ) {
		var pagecontainer,
			delegated = ( callback ),
			eventNames = eventName.split( " " );

		if( this.length === 0 ) {
			return this;
		}
		if ( !delegated ) {
			callback = element;
			element = this.selector;
			pagecontainer = $( this ).closest( ":mobile-pagecontainer" );
			if( pagecontainer.length === 0 ) {
				return this;
			}
		}
		$.each( eventNames, function( index, event ) {
			deffered.done( function(){
				if( delegated ) {
					pagecontainer = globalPagecontainer;
				}
				processEvent( event, element, callback );
			});
		});
		function processEvent( event, element, callback ) {
			if ( typeof events[ event ] === "undefined" ) {
				events[ event ] = [];
				events[ event ].push({
					element: element,
					callback: callback,
					boundTo: this
				});
				var options = {};
				options[ event ] = handleEvent;
				pagecontainer.pagecontainer(options);
			} else {
				events[ event ].push({
					element: element,
					callback: callback,
					boundTo: this
				});
			}
		}
		function handleEvent( event, ui ){
			var args = arguments;
			$.each( events[ event.type.replace( /pagecontainer/, "" ) ],
			function( index, callback ) {
				if ( from.indexOf( event.type ) !== -1 && ui.prevPage.is( callback.element ) &&
				ui.prevPage !== undefined ) {
					callback.callback.apply( ui.prevPage, args );
				} else if ( ui.toPage.is( callback.element ) ){
					callback.callback.apply( ui.toPage, args );
				}
			});
		}
		return this;
	};
	$.fn.offPage = function( removeEvents, selector, handler ) {
		if( selector === undefined ) {
			selector = this;
		}
		if( removeEvents === undefined ) {
			checkCallbacks();
			return this;
		}

		var eventArray = removeEvents.split( " " );
		$.each( eventArray, function( i, remove ) {
			checkCallbacks( remove );
		});
		return this;

		function checkCallbacks( remove ) {
			$.each( events, function( event, callbackArray ) {
				$.each( callbackArray, function( index, callback ){
					if( $( selector ).is( callback.element ) &&
					( remove? ( event === remove ) : true ) &&
					( handler? ( handler === callback.callback ): true ) ) {
						var removeIndex = callbackArray.indexOf( callback );
						array.splice( removeIndex, 1 );
					}
				});
			});
		}
	};

return $.fn;

}));
