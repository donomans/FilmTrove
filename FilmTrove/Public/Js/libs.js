/*!
 * fancyBox - jQuery Plugin
 * version: 2.0.6 (Tue, 31 Jul 2012)
 * @requires jQuery v1.6 or later
 *
 * Examples at http://fancyapps.com/fancybox/
 * License: www.fancyapps.com/fancybox/#license
 *
 * Copyright 2012 Janis Skarnelis - janis@fancyapps.com
 *
 */

(function (window, document, $, undefined) {
    "use strict";

    var W = $(window),
		D = $(document),
		F = $.fancybox = function () {
		    F.open.apply(this, arguments);
		},
		didUpdate = false,
		isTouch = document.createTouch !== undefined,

		isQuery = function (obj) {
		    return obj && obj.hasOwnProperty && obj instanceof $;
		},
		isString = function (str) {
		    return str && $.type(str) === "string";
		},
		isPercentage = function (str) {
		    return isString(str) && str.indexOf('%') > 0;
		},
		isScrollable = function (el) {
		    return (el && !(el.style.overflow && el.style.overflow === 'hidden') && ((el.clientWidth && el.scrollWidth > el.clientWidth) || (el.clientHeight && el.scrollHeight > el.clientHeight)));
		},
		getScalar = function (value, dim) {
		    value = parseInt(value, 10);

		    if (dim && isPercentage(value)) {
		        value = F.getViewport()[dim] / 100 * value;
		    }

		    return Math.ceil(value);
		},
		getValue = function (value, dim) {
		    return getScalar(value, dim) + 'px';
		};

    $.extend(F, {
        // The current version of fancyBox
        version: '2.0.6',

        defaults: {
            padding: 15,
            margin: 20,

            width: 800,
            height: 600,
            minWidth: 100,
            minHeight: 100,
            maxWidth: 9999,
            maxHeight: 9999,

            autoSize: true,
            autoHeight: false,
            autoWidth: false,

            autoResize: !isTouch,
            autoCenter: !isTouch,
            fitToView: true,
            aspectRatio: false,
            topRatio: 0.5,
            leftRatio: 0.5,

            scrolling: 'auto', // 'auto', 'yes' or 'no'
            wrapCSS: '',

            arrows: true,
            closeBtn: true,
            closeClick: false,
            nextClick: false,
            mouseWheel: true,
            autoPlay: false,
            playSpeed: 3000,
            preload: 3,
            modal: false,
            loop: true,

            ajax: {
                dataType: 'html',
                headers: { 'X-fancyBox': true }
            },
            iframe: {
                scrolling: 'auto',
                preload: true
            },
            swf: {
                wmode: 'transparent',
                allowfullscreen: 'true',
                allowscriptaccess: 'always'
            },

            keys: {
                next: {
                    13: 'right', // enter
                    34: 'down',  // page down
                    39: 'right', // right arrow
                    40: 'down'   // down arrow
                },
                prev: {
                    8: 'left', // backspace
                    33: 'up',   // page up
                    37: 'left', // left arrow
                    38: 'up'    // up arrow
                },
                close: [27], // escape key
                play: [32], // space - start/stop slideshow
                toggle: [70]  // letter "f" - toggle fullscreen
            },

            direction: {
                next: 'right',
                prev: 'left'
            },

            scrollOutside: true,

            // Override some properties
            index: 0,
            type: null,
            href: null,
            content: null,
            title: null,

            // HTML templates
            tpl: {
                wrap: '<div class="fancybox-wrap" tabIndex="-1"><div class="fancybox-skin"><div class="fancybox-outer"><div class="fancybox-inner"></div></div></div></div>',
                image: '<img class="fancybox-image" src="{href}" alt="" />',
                iframe: '<iframe id="fancybox-frame{rnd}" name="fancybox-frame{rnd}" class="fancybox-iframe" frameborder="0" vspace="0" hspace="0"' + ($.browser.msie ? ' allowtransparency="true"' : '') + '></iframe>',
                error: '<p class="fancybox-error">The requested content cannot be loaded.<br/>Please try again later.</p>',
                closeBtn: '<div title="Close" class="fancybox-item fancybox-close"></div>',
                next: '<a title="Next" class="fancybox-nav fancybox-next"><span></span></a>',
                prev: '<a title="Previous" class="fancybox-nav fancybox-prev"><span></span></a>'
            },

            // Properties for each animation type
            // Opening fancyBox
            openEffect: 'fade', // 'elastic', 'fade' or 'none'
            openSpeed: 250,
            openEasing: 'swing',
            openOpacity: true,
            openMethod: 'zoomIn',

            // Closing fancyBox
            closeEffect: 'fade', // 'elastic', 'fade' or 'none'
            closeSpeed: 250,
            closeEasing: 'swing',
            closeOpacity: true,
            closeMethod: 'zoomOut',

            // Changing next gallery item
            nextEffect: 'elastic', // 'elastic', 'fade' or 'none'
            nextSpeed: 250,
            nextEasing: 'swing',
            nextMethod: 'changeIn',

            // Changing previous gallery item
            prevEffect: 'elastic', // 'elastic', 'fade' or 'none'
            prevSpeed: 250,
            prevEasing: 'swing',
            prevMethod: 'changeOut',

            // Enabled helpers
            helpers: {
                overlay: {
                    closeClick: true,
                    speedOut: 200,
                    showEarly: false,
                    css: {
                        'background-color': 'rgba(0,0,0,0.5)'
                    }
                },
                title: {
                    type: 'float' // 'float', 'inside', 'outside' or 'over'
                }
            },

            // Callbacks
            onCancel: $.noop, // If canceling
            beforeLoad: $.noop, // Before loading
            afterLoad: $.noop, // After loading
            beforeShow: $.noop, // Before changing in current item
            afterShow: $.noop, // After opening
            beforeChange: $.noop, // Before changing gallery item
            beforeClose: $.noop, // Before closing
            afterClose: $.noop  // After closing
        },

        //Current state
        group: {}, // Selected group
        opts: {}, // Group options
        previous: null,  // Previous element
        coming: null,  // Element being loaded
        current: null,  // Currently loaded element
        isActive: false, // Is activated
        isOpen: false, // Is currently open
        isOpened: false, // Have been fully opened at least once

        wrap: null,
        skin: null,
        outer: null,
        inner: null,

        player: {
            timer: null,
            isActive: false
        },

        // Loaders
        ajaxLoad: null,
        imgPreload: null,

        // Some collections
        transitions: {},
        helpers: {},

        /*
		 *	Static methods
		 */

        open: function (group, opts) {
            if (!group) {
                return;
            }

            if (!$.isPlainObject(opts)) {
                opts = {};
            }

            // Close if already active
            if (false === F.close(true)) {
                return;
            }

            // Normalize group
            if (!$.isArray(group)) {
                group = isQuery(group) ? $(group).get() : [group];
            }

            // Recheck if the type of each element is `object` and set content type (image, ajax, etc)
            $.each(group, function (i, element) {
                var obj = {},
					href,
					title,
					content,
					type,
					rez,
					hrefParts,
					selector;

                if ($.type(element) === "object") {
                    // Check if is DOM element
                    if (element.nodeType) {
                        element = $(element);
                    }

                    if (isQuery(element)) {
                        obj = {
                            href: element.attr('href'),
                            title: element.attr('title'),
                            isDom: true,
                            element: element
                        };

                        if ($.metadata) {
                            $.extend(true, obj, element.metadata());
                        }

                    } else {
                        obj = element;
                    }
                }

                href = opts.href || obj.href || (isString(element) ? element : null);
                title = opts.title !== undefined ? opts.title : obj.title || '';

                content = opts.content || obj.content;
                type = content ? 'html' : (opts.type || obj.type);

                if (!type && obj.isDom) {
                    type = element.data('fancybox-type');

                    if (!type) {
                        rez = element.prop('class').match(/fancybox\.(\w+)/);
                        type = rez ? rez[1] : null;
                    }
                }

                if (isString(href)) {
                    // Try to guess the content type
                    if (!type) {
                        if (F.isImage(href)) {
                            type = 'image';

                        } else if (F.isSWF(href)) {
                            type = 'swf';

                        } else if (href.charAt(0) === '#') {
                            type = 'inline';

                        } else if (isString(element)) {
                            type = 'html';
                            content = element;
                        }
                    }

                    // Split url into two pieces with source url and content selector, e.g,
                    // "/mypage.html #my_id" will load "/mypage.html" and display element having id "my_id"
                    if (type === 'ajax') {
                        hrefParts = href.split(/\s+/, 2);
                        href = hrefParts.shift();
                        selector = hrefParts.shift();
                    }
                }

                if (!content) {
                    if (type === 'inline') {
                        if (href) {
                            content = $(isString(href) ? href.replace(/.*(?=#[^\s]+$)/, '') : href); //strip for ie7

                        } else if (obj.isDom) {
                            content = element;
                        }

                    } else if (type === 'html') {
                        content = href;

                    } else if (!type && !href && obj.isDom) {
                        type = 'inline';
                        content = element;
                    }
                }

                $.extend(obj, {
                    href: href,
                    type: type,
                    content: content,
                    title: title,
                    selector: selector
                });

                group[i] = obj;
            });

            // Extend the defaults
            F.opts = $.extend(true, {}, F.defaults, opts);

            // All options are merged recursive except keys
            if (opts.keys !== undefined) {
                F.opts.keys = opts.keys ? $.extend({}, F.defaults.keys, opts.keys) : false;
            }

            F.group = group;

            return F._start(F.opts.index || 0);
        },

        // Cancel image loading or abort ajax request
        cancel: function () {
            var coming = F.coming;

            if (!coming || false === F.trigger('onCancel')) {
                return;
            }

            F.hideLoading();

            if (F.ajaxLoad) {
                F.ajaxLoad.abort();
            }

            F.ajaxLoad = null;

            if (F.imgPreload) {
                F.imgPreload.onload = F.imgPreload.onerror = null;
            }

            // If the first item has been canceled, then clear everything
            if (coming.wrap) {
                coming.wrap.stop(true).trigger('onReset').remove();
            }

            if (!F.current) {
                F.trigger('afterClose');
            }

            F.coming = null;
        },

        // Start closing animation if is open; remove immediately if opening/closing
        close: function (immediately) {
            F.cancel();

            if (false === F.trigger('beforeClose')) {
                return;
            }

            F.unbindEvents();

            if (!F.isOpen || immediately === true) {
                $('.fancybox-wrap').stop(true).trigger('onReset').remove();

                F._afterZoomOut();

            } else {
                F.isOpen = F.isOpened = false;
                F.isClosing = true;

                $('.fancybox-item, .fancybox-nav').remove();

                F.wrap.stop(true).removeClass('fancybox-opened');

                if (F.wrap.css('position') === 'fixed') {
                    F.wrap.css(F._getPosition(true));
                }

                F.transitions[F.current.closeMethod]();
            }
        },

        // Manage slideshow:
        //   $.fancybox.play(); - toggle slideshow
        //   $.fancybox.play( true ); - start
        //   $.fancybox.play( false ); - stop
        play: function (action) {
            var clear = function () {
                clearTimeout(F.player.timer);
            },
				set = function () {
				    clear();

				    if (F.current && F.player.isActive) {
				        F.player.timer = setTimeout(F.next, F.current.playSpeed);
				    }
				},
				stop = function () {
				    clear();

				    $('body').unbind('.player');

				    F.player.isActive = false;

				    F.trigger('onPlayEnd');
				},
				start = function () {
				    if (F.current && (F.current.loop || F.current.index < F.group.length - 1)) {
				        F.player.isActive = true;

				        $('body').bind({
				            'afterShow.player onUpdate.player': set,
				            'onCancel.player beforeClose.player': stop,
				            'beforeLoad.player': clear
				        });

				        set();

				        F.trigger('onPlayStart');
				    }
				};

            if (action === true || (!F.player.isActive && action !== false)) {
                start();
            } else {
                stop();
            }
        },

        // Navigate to next gallery item
        next: function (direction) {
            var current = F.current;

            if (current) {
                if (!isString(direction)) {
                    direction = current.direction.next;
                }

                F.jumpto(current.index + 1, direction, 'next');
            }
        },

        // Navigate to previous gallery item
        prev: function (direction) {
            var current = F.current;

            if (current) {
                if (!isString(direction)) {
                    direction = current.direction.prev;
                }

                F.jumpto(current.index - 1, direction, 'prev');
            }
        },

        // Navigate to gallery item by index
        jumpto: function (index, direction, router) {
            var current = F.current;

            if (!current) {
                return;
            }

            index = getScalar(index);

            F.direction = direction || (index > current.index ? 'right' : 'left');
            F.router = router || 'jumpto';

            if (current.loop) {
                if (index < 0) {
                    index = current.group.length + (index % current.group.length);
                }

                index = index % current.group.length;
            }

            if (current.group[index] !== undefined) {
                F.cancel();

                F._start(index);
            }
        },

        // Center inside viewport and toggle position type to fixed or absolute if needed
        reposition: function (e, onlyAbsolute) {
            var pos;

            if (F.isOpen) {
                pos = F._getPosition(onlyAbsolute);

                if (e && e.type === 'scroll') {
                    delete pos.position;

                    F.wrap.stop(true, true).animate(pos, 200);

                } else {
                    F.wrap.css(pos);
                }
            }
        },

        update: function (e) {
            var anyway = !e || (e && e.type === 'orientationchange'),
				scroll = e && e.type === 'scroll';

            if (anyway) {
                clearTimeout(didUpdate);

                didUpdate = null;
            }

            if (!F.isOpen || didUpdate) {
                return;
            }

            // Touch devices need some help to restore document dimensions
            if (anyway && isTouch) {
                F.wrap.removeAttr('style').addClass('fancybox-tmp');

                F.trigger('onUpdate');
            }

            didUpdate = setTimeout(function () {
                var current = F.current;

                didUpdate = null;

                if (!current) {
                    return;
                }

                F.wrap.removeClass('fancybox-tmp');

                if ((current.autoResize && !scroll) || anyway) {
                    F._setDimension();

                    F.trigger('onUpdate');
                }

                if ((current.autoCenter && !(scroll && current.canShrink)) || anyway) {
                    F.reposition(e);
                }

                F.trigger('onUpdate');

            }, (anyway ? 20 : 300));
        },

        // Shrink content to fit inside viewport or restore if resized
        toggle: function (action) {
            if (F.isOpen) {
                F.current.fitToView = $.type(action) === "boolean" ? action : !F.current.fitToView;

                F.update();
            }
        },

        hideLoading: function () {
            D.unbind('keypress.fb');

            $('#fancybox-loading').remove();
        },

        showLoading: function () {
            var el, viewport;

            F.hideLoading();

            // If user will press the escape-button, the request will be canceled
            D.bind('keypress.fb', function (e) {
                if ((e.which || e.keyCode) === 27) {
                    e.preventDefault();
                    F.cancel();
                }
            });

            el = $('<div id="fancybox-loading"><div></div></div>').click(F.cancel).appendTo('body');

            if (!F.defaults.fixed) {
                viewport = F.getViewport();

                el.css({
                    position: 'absolute',
                    top: (viewport.h * 0.5) + viewport.y,
                    left: (viewport.w * 0.5) + viewport.x
                });
            }
        },

        getViewport: function () {
            // See http://bugs.jquery.com/ticket/6724
            return {
                x: W.scrollLeft(),
                y: W.scrollTop(),
                w: (isTouch && window.innerWidth ? window.innerWidth : W.width()) - F.defaults.scrollbarWidth,
                h: isTouch && window.innerHeight ? window.innerHeight : W.height()
            };
        },

        // Unbind the keyboard / clicking actions
        unbindEvents: function () {
            if (F.wrap && isQuery(F.wrap)) {
                F.wrap.unbind('.fb');
            }

            D.unbind('.fb');
            W.unbind('.fb');
        },

        bindEvents: function () {
            var current = F.current,
				keys;

            if (!current) {
                return;
            }

            W.bind('resize.fb orientationchange.fb' + (current.autoCenter && !current.locked ? ' scroll.fb' : ''), F.update);

            keys = current.keys;

            if (keys) {
                D.bind('keydown.fb', function (e) {
                    var code = e.which || e.keyCode,
						target = e.target || e.srcElement;

                    // Ignore key combinations and key events within form elements
                    if (!e.ctrlKey && !e.altKey && !e.shiftKey && !e.metaKey && !(target && (target.type || $(target).is('[contenteditable]')))) {
                        $.each(keys, function (i, val) {
                            if (current.group.length > 1 && val[code] !== undefined) {
                                F[i](val[code]);

                                e.preventDefault();
                                return false;
                            }

                            if ($.inArray(code, val) > -1) {
                                F[i]();

                                e.preventDefault();
                                return false;
                            }
                        });
                    }
                });
            }

            if ($.fn.mousewheel && current.mouseWheel) {
                F.wrap.bind('mousewheel.fb', function (e, delta, deltaX, deltaY) {
                    var target = e.target || null,
						parent = $(target),
						canScroll = false;

                    while (parent.length) {
                        if (canScroll || parent.is('.fancybox-skin') || parent.is('.fancybox-wrap')) {
                            break;
                        }

                        canScroll = isScrollable(parent[0]);
                        parent = $(parent).parent();
                    }

                    if (delta !== 0 && !canScroll) {
                        if (F.group.length > 1 && !current.canShrink) {
                            if (deltaY > 0 || deltaX > 0) {
                                F.prev(deltaY > 0 ? 'up' : 'left');

                            } else if (deltaY < 0 || deltaX < 0) {
                                F.next(deltaY < 0 ? 'down' : 'right');
                            }

                            e.preventDefault();
                        }
                    }
                });
            }
        },

        trigger: function (event, o) {
            var ret, obj = o || F.coming || F.current;

            if (!obj) {
                return;
            }

            if ($.isFunction(obj[event])) {
                ret = obj[event].apply(obj, Array.prototype.slice.call(arguments, 1));
            }

            if (ret === false) {
                return false;
            }

            if (event === 'onCancel' && !F.isOpened) {
                F.isActive = false;
            }

            if (obj.helpers) {
                $.each(obj.helpers, function (helper, opts) {
                    if (opts && F.helpers[helper] && $.isFunction(F.helpers[helper][event])) {
                        F.helpers[helper][event](opts, obj);
                    }
                });
            }

            $.event.trigger(event + '.fb');
        },

        isImage: function (str) {
            return isString(str) && str.match(/\.(jp(e|g|eg)|gif|png|bmp|webp)((\?|#).*)?$/i);
        },

        isSWF: function (str) {
            return isString(str) && str.match(/\.(swf)((\?|#).*)?$/i);
        },

        _start: function (index) {
            var coming = {},
				obj = F.group[index] || null,
				href,
				type,
				margin,
				padding;

            if (!obj) {
                return false;
            }

            coming = $.extend(true, {}, F.opts, obj);

            // Convert margin and padding properties to array - top, right, bottom, left
            margin = coming.margin;
            padding = coming.padding;

            if ($.type(margin) === 'number') {
                coming.margin = [margin, margin, margin, margin];
            }

            if ($.type(padding) === 'number') {
                coming.padding = [padding, padding, padding, padding];
            }

            // 'modal' propery is just a shortcut
            if (coming.modal) {
                $.extend(true, coming, {
                    closeBtn: false,
                    closeClick: false,
                    nextClick: false,
                    arrows: false,
                    mouseWheel: false,
                    keys: null,
                    helpers: {
                        overlay: {
                            closeClick: false
                        }
                    }
                });
            }

            // 'autoSize' property is a shortcut, too
            if (coming.autoSize) {
                coming.autoWidth = coming.autoHeight = true;
            }

            if (coming.width === 'auto') {
                coming.autoWidth = true;
            }

            if (coming.height === 'auto') {
                coming.autoHeight = true;
            }

            /*
			 * Add reference to the group, so it`s possible to access from callbacks, example:
			 * afterLoad : function() {
			 *     this.title = 'Image ' + (this.index + 1) + ' of ' + this.group.length + (this.title ? ' - ' + this.title : '');
			 * }
			 */

            coming.group = F.group;
            coming.index = index;

            // Give a chance for callback or helpers to update coming item (type, title, etc)
            F.coming = coming;

            if (false === F.trigger('beforeLoad')) {
                F.coming = null;

                return;
            }

            type = coming.type;
            href = coming.href;

            if (!type) {
                F.coming = null;

                //If we can not determine content type then drop silently or display next/prev item if looping through gallery
                if (F.current && F.router && F.router !== 'jumpto') {
                    F.current.index = index;

                    return F[F.router](F.direction);
                }

                return false;
            }

            F.isActive = true;

            if (type === 'image' || type === 'swf') {
                coming.autoHeight = coming.autoWidth = false;
                coming.scrolling = 'visible';
            }

            if (type === 'image') {
                coming.aspectRatio = true;
            }

            if (type === 'iframe' && isTouch) {
                coming.scrolling = 'scroll';
            }

            // Build the neccessary markup
            coming.wrap = $(coming.tpl.wrap).addClass('fancybox-' + (isTouch ? 'mobile' : 'desktop') + ' fancybox-type-' + type + ' fancybox-tmp ' + coming.wrapCSS).appendTo(coming.parent);

            $.extend(coming, {
                skin: $('.fancybox-skin', coming.wrap),
                outer: $('.fancybox-outer', coming.wrap),
                inner: $('.fancybox-inner', coming.wrap)
            });

            $.each(["Top", "Right", "Bottom", "Left"], function (i, v) {
                coming.skin.css('padding' + v, getValue(coming.padding[i]));
            });

            F.trigger('onReady');

            // Check before try to load; 'inline' and 'html' types need content, others - href
            if (type === 'inline' || type === 'html') {
                if (!coming.content || !coming.content.length) {
                    return F._error('content');
                }

            } else if (!href) {
                return F._error('href');
            }

            if (type === 'image') {
                F._loadImage();

            } else if (type === 'ajax') {
                F._loadAjax();

            } else if (type === 'iframe') {
                F._loadIframe();

            } else {
                F._afterLoad();
            }
        },

        _error: function (type) {
            $.extend(F.coming, {
                type: 'html',
                autoWidth: true,
                autoHeight: true,
                minWidth: 0,
                minHeight: 0,
                scrolling: 'no',
                hasError: type,
                content: F.coming.tpl.error
            });

            F._afterLoad();
        },

        _loadImage: function () {
            // Reset preload image so it is later possible to check "complete" property
            var img = F.imgPreload = new Image();

            img.onload = function () {
                this.onload = this.onerror = null;

                F.coming.width = this.width;
                F.coming.height = this.height;

                F._afterLoad();
            };

            img.onerror = function () {
                this.onload = this.onerror = null;

                F._error('image');
            };

            img.src = F.coming.href;

            if (img.complete === undefined || !img.complete) {
                F.showLoading();
            }
        },

        _loadAjax: function () {
            var coming = F.coming;

            F.showLoading();

            F.ajaxLoad = $.ajax($.extend({}, coming.ajax, {
                url: coming.href,
                error: function (jqXHR, textStatus) {
                    if (F.coming && textStatus !== 'abort') {
                        F._error('ajax', jqXHR);

                    } else {
                        F.hideLoading();
                    }
                },
                success: function (data, textStatus) {
                    if (textStatus === 'success') {
                        coming.content = data;

                        F._afterLoad();
                    }
                }
            }));
        },

        _loadIframe: function () {
            var coming = F.coming,
				iframe = $(coming.tpl.iframe.replace(/\{rnd\}/g, new Date().getTime()))
					.attr('scrolling', isTouch ? 'auto' : coming.iframe.scrolling)
					.attr('src', coming.href);

            // This helps IE
            $(coming.wrap).bind('onReset', function () {
                try {
                    $(this).find('iframe').hide().attr('src', '//about:blank').end().empty();
                } catch (e) { }
            });

            if (coming.iframe.preload) {
                F.showLoading();

                iframe.one('load', function () {
                    $(this).bind('load.fb', F.update).data('ready', 1);

                    // Without this trick:
                    //   - iframe won't scroll on iOS devices
                    //   - IE7 sometimes displays empty iframe
                    $(this).parents('.fancybox-wrap').width('100%').removeClass('fancybox-tmp').show();

                    F._afterLoad();
                });
            }

            coming.content = iframe.appendTo(coming.inner);

            if (!coming.iframe.preload) {
                F._afterLoad();
            }
        },

        _preloadImages: function () {
            var group = F.group,
				current = F.current,
				len = group.length,
				cnt = current.preload ? Math.min(current.preload, len - 1) : 0,
				item,
				i;

            for (i = 1; i <= cnt; i += 1) {
                item = group[(current.index + i) % len];

                if (item.type === 'image' && item.href) {
                    new Image().src = item.href;
                }
            }
        },

        _afterLoad: function () {
            var coming = F.coming,
				previous = F.current,
				placeholder = 'fancybox-placeholder',
				current,
				content,
				type,
				scrolling,
				href,
				embed;

            F.hideLoading();

            if (!coming || F.isActive === false) {
                return;
            }

            if (false === F.trigger('afterLoad', coming, previous)) {
                coming.wrap.stop(true).trigger('onReset').remove();

                F.coming = null;

                return;
            }

            if (previous) {
                F.trigger('beforeChange', previous);

                previous.wrap.stop(true).removeClass('fancybox-opened')
					.find('.fancybox-item, .fancybox-nav')
					.remove();

                if (previous.wrap.css('position') === 'fixed') {
                    previous.wrap.css(F._getPosition(true));
                }
            }

            F.unbindEvents();

            current = coming;
            content = coming.content;
            type = coming.type;
            scrolling = coming.scrolling;

            $.extend(F, {
                wrap: current.wrap,
                skin: current.skin,
                outer: current.outer,
                inner: current.inner,
                current: current,
                previous: previous
            });

            href = current.href;

            switch (type) {
                case 'inline':
                case 'ajax':
                case 'html':
                    if (current.selector) {
                        content = $('<div>').html(content).find(current.selector);

                    } else if (isQuery(content)) {
                        if (!content.data(placeholder)) {
                            content.data(placeholder, $('<div class="' + placeholder + '"></div>').insertAfter(content).hide());
                        }

                        content = content.show().detach();

                        current.wrap.bind('onReset', function () {
                            if ($(this).find(content).length) {
                                content.hide().replaceAll(content.data(placeholder)).data(placeholder, false);
                            }
                        });
                    }
                    break;

                case 'image':
                    content = current.tpl.image.replace('{href}', href);
                    break;

                case 'swf':
                    content = '<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" width="100%" height="100%"><param name="movie" value="' + href + '"></param>';
                    embed = '';

                    $.each(current.swf, function (name, val) {
                        content += '<param name="' + name + '" value="' + val + '"></param>';
                        embed += ' ' + name + '="' + val + '"';
                    });

                    content += '<embed src="' + href + '" type="application/x-shockwave-flash" width="100%" height="100%"' + embed + '></embed></object>';
                    break;
            }

            if (!(isQuery(content) && content.parent().is(current.inner))) {
                current.inner.append(content);
            }

            // Give a chance for helpers or callbacks to update elements
            F.trigger('beforeShow');

            // Set initial dimensions and start position
            F._setDimension();

            current.wrap.removeClass('fancybox-tmp');

            current.inner.css('overflow', scrolling === 'yes' ? 'scroll' : (scrolling === 'no' ? 'hidden' : scrolling));

            current.pos = $.extend({}, current.dim, F._getPosition(true));

            F.isOpen = false;
            F.coming = null;

            F.bindEvents();

            if (!F.isOpened) {
                $('.fancybox-wrap').not(current.wrap).stop(true).trigger('onReset').remove();

            } else if (previous.prevMethod) {
                F.transitions[previous.prevMethod]();
            }

            F.transitions[F.isOpened ? current.nextMethod : current.openMethod]();

            F._preloadImages();
        },

        _setDimension: function () {
            var viewport = F.getViewport(),
				steps = 0,
				canShrink = false,
				canExpand = false,
				wrap = F.wrap,
				skin = F.skin,
				inner = F.inner,
				current = F.current,
				width = current.width,
				height = current.height,
				minWidth = current.minWidth,
				minHeight = current.minHeight,
				maxWidth = current.maxWidth,
				maxHeight = current.maxHeight,
				scrolling = current.scrolling,
				scrollOut = current.scrollOutside ? current.scrollbarWidth : 0,
				margin = current.margin,
				wMargin = margin[1] + margin[3],
				hMargin = margin[0] + margin[2],
				wPadding,
				hPadding,
				wSpace,
				hSpace,
				origWidth,
				origHeight,
				origMaxWidth,
				origMaxHeight,
				ratio,
				width_,
				height_,
				maxWidth_,
				maxHeight_,
				iframe,
				body;

            // Reset dimensions so we could re-check actual size
            wrap.add(skin).add(inner).width('auto').height('auto');

            wPadding = skin.outerWidth(true) - skin.width();
            hPadding = skin.outerHeight(true) - skin.height();

            // Any space between content and viewport (margin, padding, border, title)
            wSpace = wMargin + wPadding;
            hSpace = hMargin + hPadding;

            origWidth = isPercentage(width) ? (viewport.w - wSpace) * getScalar(width) / 100 : width;
            origHeight = isPercentage(height) ? (viewport.h - hSpace) * getScalar(height) / 100 : height;

            if (current.type === 'iframe') {
                iframe = current.content;

                if (current.autoHeight && iframe.data('ready') === 1) {
                    try {
                        if (iframe[0].contentWindow.document.location) {
                            inner.width(origWidth).height(9999);

                            body = iframe.contents().find('body');

                            if (scrollOut) {
                                body.css('overflow-x', 'hidden');
                            }

                            origHeight = body.height();
                        }

                    } catch (e) { }
                }

            } else if (current.autoWidth || current.autoHeight) {
                inner.addClass('fancybox-tmp');

                // Set width or height in case we need to calculate only one dimension
                if (!current.autoWidth) {
                    inner.width(origWidth);
                }

                if (!current.autoHeight) {
                    inner.height(origHeight);
                }

                if (current.autoWidth) {
                    origWidth = inner.width();
                }

                if (current.autoHeight) {
                    origHeight = inner.height();
                }

                inner.removeClass('fancybox-tmp');
            }

            width = getScalar(origWidth);
            height = getScalar(origHeight);

            ratio = origWidth / origHeight;

            // Calculations for the content
            minWidth = getScalar(isPercentage(minWidth) ? getScalar(minWidth, 'w') - wSpace : minWidth);
            maxWidth = getScalar(isPercentage(maxWidth) ? getScalar(maxWidth, 'w') - wSpace : maxWidth);

            minHeight = getScalar(isPercentage(minHeight) ? getScalar(minHeight, 'h') - hSpace : minHeight);
            maxHeight = getScalar(isPercentage(maxHeight) ? getScalar(maxHeight, 'h') - hSpace : maxHeight);

            // These will be used to determine if wrap can fit in the viewport
            origMaxWidth = maxWidth;
            origMaxHeight = maxHeight;

            maxWidth_ = viewport.w - wMargin;
            maxHeight_ = viewport.h - hMargin;

            if (current.aspectRatio) {
                if (width > maxWidth) {
                    width = maxWidth;
                    height = width / ratio;
                }

                if (height > maxHeight) {
                    height = maxHeight;
                    width = height * ratio;
                }

                if (width < minWidth) {
                    width = minWidth;
                    height = width / ratio;
                }

                if (height < minHeight) {
                    height = minHeight;
                    width = height * ratio;
                }

            } else {
                width = Math.max(minWidth, Math.min(width, maxWidth));
                height = Math.max(minHeight, Math.min(height, maxHeight));
            }

            // Try to fit inside viewport (including the title)
            if (current.fitToView) {
                maxWidth = Math.min(viewport.w - wSpace, maxWidth);
                maxHeight = Math.min(viewport.h - hSpace, maxHeight);

                inner.width(getScalar(width)).height(getScalar(height));

                wrap.width(getScalar(width + wPadding));

                // Real wrap dimensions
                width_ = wrap.width();
                height_ = wrap.height();

                if (current.aspectRatio) {
                    while ((width_ > maxWidth_ || height_ > maxHeight_) && width > minWidth && height > minHeight) {
                        if (steps++ > 19) {
                            break;
                        }

                        height = Math.max(minHeight, Math.min(maxHeight, height - 10));
                        width = height * ratio;

                        if (width < minWidth) {
                            width = minWidth;
                            height = width / ratio;
                        }

                        if (width > maxWidth) {
                            width = maxWidth;
                            height = width / ratio;
                        }

                        inner.width(getScalar(width)).height(getScalar(height));

                        wrap.width(getScalar(width + wPadding));

                        width_ = wrap.width();
                        height_ = wrap.height();
                    }

                } else {
                    width = Math.max(minWidth, Math.min(width, width - (width_ - maxWidth_)));
                    height = Math.max(minHeight, Math.min(height, height - (height_ - maxHeight_)));
                }
            }

            if (scrollOut && scrolling === 'auto' && height < origHeight && (width + wPadding + scrollOut) < maxWidth_) {
                width += scrollOut;
            }

            inner.width(getScalar(width)).height(getScalar(height));

            wrap.width(getScalar(width + wPadding));

            width_ = wrap.width();
            height_ = wrap.height();

            canShrink = (width_ > maxWidth_ || height_ > maxHeight_) && width > minWidth && height > minHeight;
            canExpand = current.aspectRatio ? (width < origMaxWidth && height < origMaxHeight && width < origWidth && height < origHeight) : ((width < origMaxWidth || height < origMaxHeight) && (width < origWidth || height < origHeight));

            $.extend(current, {
                dim: {
                    width: getValue(width_),
                    height: getValue(height_)
                },
                origWidth: origWidth,
                origHeight: origHeight,
                canShrink: canShrink,
                canExpand: canExpand,
                wPadding: wPadding,
                hPadding: hPadding,
                wrapSpace: height_ - skin.outerHeight(true),
                skinSpace: skin.height() - height
            });

            if (!iframe && current.autoHeight && height > minHeight && height < maxHeight && !canExpand) {
                inner.height('auto');
            }
        },

        _getPosition: function (onlyAbsolute) {
            var current = F.current,
				viewport = F.getViewport(),
				margin = current.margin,
				width = F.wrap.width() + margin[1] + margin[3],
				height = F.wrap.height() + margin[0] + margin[2],
				rez = {
				    position: 'absolute',
				    top: margin[0],
				    left: margin[3]
				};

            if (current.autoCenter && current.fixed && !onlyAbsolute && height <= viewport.h && width <= viewport.w) {
                rez.position = 'fixed';

            } else if (current.locked !== true) {
                rez.top += viewport.y;
                rez.left += viewport.x;
            }

            rez.top = getValue(Math.max(rez.top, rez.top + ((viewport.h - height) * current.topRatio)));
            rez.left = getValue(Math.max(rez.left, rez.left + ((viewport.w - width) * current.leftRatio)));

            return rez;
        },

        _afterZoomIn: function () {
            var current = F.current;

            if (!current) {
                return;
            }

            F.isOpen = F.isOpened = true;

            F.wrap.addClass('fancybox-opened').css('overflow', 'visible');

            F.reposition();

            // Assign a click event
            if (current.closeClick || current.nextClick) {
                F.inner.css('cursor', 'pointer').bind('click.fb', function (e) {
                    if (!$(e.target).is('a') && !$(e.target).parent().is('a')) {
                        F[current.closeClick ? 'close' : 'next']();
                    }
                });
            }

            // Create a close button
            if (current.closeBtn) {
                $(current.tpl.closeBtn).appendTo(F.skin).bind('click.fb', F.close);
            }

            // Create navigation arrows
            if (current.arrows && F.group.length > 1) {
                if (current.loop || current.index > 0) {
                    $(current.tpl.prev).appendTo(F.outer).bind('click.fb', F.prev);
                }

                if (current.loop || current.index < F.group.length - 1) {
                    $(current.tpl.next).appendTo(F.outer).bind('click.fb', F.next);
                }
            }

            current.wrap.focus();

            F.trigger('afterShow');

            // Stop the slideshow if this is the last item
            if (!current.loop && current.index === current.group.length - 1) {
                F.play(false);

            } else if (F.opts.autoPlay && !F.player.isActive) {
                F.opts.autoPlay = false;

                F.play();
            }
        },

        _afterZoomOut: function () {
            var current = F.current;

            $('.fancybox-wrap').stop(true).trigger('onReset').remove();

            $.extend(F, {
                group: {},
                opts: {},
                router: false,
                current: null,
                isActive: false,
                isOpened: false,
                isOpen: false,
                isClosing: false,
                wrap: null,
                skin: null,
                outer: null,
                inner: null
            });

            F.trigger('afterClose', current);
        }
    });

    /*
	 *	Default transitions
	 */

    F.transitions = {
        getOrigPosition: function () {
            var current = F.current,
				element = current.element,
				orig = current.orig,
				pos = {},
				width = 50,
				height = 50,
				hPadding = current.hPadding,
				wPadding = current.wPadding,
				viewport = F.getViewport();

            if (!orig && current.isDom && element.is(':visible')) {
                orig = element.find('img:first');

                if (!orig.length) {
                    orig = element;
                }
            }

            if (isQuery(orig)) {
                pos = orig.offset();

                if (orig.is('img')) {
                    width = orig.outerWidth();
                    height = orig.outerHeight();
                }

            } else {
                pos.top = viewport.y + (viewport.h - height) * current.topRatio;
                pos.left = viewport.x + (viewport.w - width) * current.leftRatio;
            }

            if (current.locked === true) {
                pos.top -= viewport.y;
                pos.left -= viewport.x;
            }

            pos = {
                top: getValue(pos.top - hPadding * current.topRatio),
                left: getValue(pos.left - wPadding * current.leftRatio),
                width: getValue(width + wPadding),
                height: getValue(height + hPadding)
            };

            return pos;
        },

        step: function (now, fx) {
            var ratio,
				padding,
				value,
				prop = fx.prop,
				current = F.current,
				wrapSpace = current.wrapSpace,
				skinSpace = current.skinSpace;

            if (prop === 'width' || prop === 'height') {
                ratio = fx.end === fx.start ? 1 : (now - fx.start) / (fx.end - fx.start);

                if (F.isClosing) {
                    ratio = 1 - ratio;
                }

                padding = prop === 'width' ? current.wPadding : current.hPadding;
                value = now - padding;

                F.skin[prop](getScalar(prop === 'width' ? value : value - (wrapSpace * ratio)));
                F.inner[prop](getScalar(prop === 'width' ? value : value - (wrapSpace * ratio) - (skinSpace * ratio)));
            }
        },

        zoomIn: function () {
            var current = F.current,
				startPos = current.pos,
				effect = current.openEffect,
				elastic = effect === 'elastic',
				endPos = $.extend({ opacity: 1 }, startPos);

            // Remove "position" property that breaks older IE
            delete endPos.position;

            if (elastic) {
                startPos = this.getOrigPosition();

                if (current.openOpacity) {
                    startPos.opacity = 0.1;
                }

            } else if (effect === 'fade') {
                startPos.opacity = 0.1;
            }

            F.wrap.css(startPos).animate(endPos, {
                duration: effect === 'none' ? 0 : current.openSpeed,
                easing: current.openEasing,
                step: elastic ? this.step : null,
                complete: F._afterZoomIn
            });
        },

        zoomOut: function () {
            var current = F.current,
				effect = current.closeEffect,
				elastic = effect === 'elastic',
				endPos = { opacity: 0.1 };

            if (elastic) {
                endPos = this.getOrigPosition();

                if (current.closeOpacity) {
                    endPos.opacity = 0.1;
                }
            }

            F.wrap.animate(endPos, {
                duration: effect === 'none' ? 0 : current.closeSpeed,
                easing: current.closeEasing,
                step: elastic ? this.step : null,
                complete: F._afterZoomOut
            });
        },

        changeIn: function () {
            var current = F.current,
				effect = current.nextEffect,
				startPos = current.pos,
				endPos = { opacity: 1 },
				direction = F.direction,
				distance = 200,
				field;

            startPos.opacity = 0.1;

            if (effect === 'elastic') {
                field = direction === 'down' || direction === 'up' ? 'top' : 'left';

                if (direction === 'up' || direction === 'left') {
                    startPos[field] = getValue(getScalar(startPos[field]) - distance);
                    endPos[field] = '+=' + distance + 'px';

                } else {
                    startPos[field] = getValue(getScalar(startPos[field]) + distance);
                    endPos[field] = '-=' + distance + 'px';
                }
            }

            F.wrap.css(startPos).animate(endPos, {
                duration: effect === 'none' ? 0 : current.nextSpeed,
                easing: current.nextEasing,
                complete: function () {
                    setTimeout(F._afterZoomIn, 10);
                }
            });
        },

        changeOut: function () {
            var previous = F.previous,
				effect = previous.prevEffect,
				endPos = { opacity: 0.1 },
				direction = F.direction,
				distance = 200;

            if (effect === 'elastic') {
                endPos[direction === 'down' || direction === 'up' ? 'top' : 'left'] = (direction === 'down' || direction === 'right' ? '-' : '+') + '=' + distance + 'px';
            }

            previous.wrap.animate(endPos, {
                duration: effect === 'none' ? 0 : previous.prevSpeed,
                easing: previous.prevEasing,
                complete: function () {
                    $(this).trigger('onReset').remove();
                }
            });
        }
    };

    /*
	 *	Overlay helper
	 */

    F.helpers.overlay = {
        overlay: null,

        update: function () {
            var width, scrollWidth, offsetWidth;

            // Reset width/height so it will not mess
            this.overlay.width('100%').height('100%');

            if ($.browser.msie || isTouch) {
                scrollWidth = Math.max(document.documentElement.scrollWidth, document.body.scrollWidth);
                offsetWidth = Math.max(document.documentElement.offsetWidth, document.body.offsetWidth);

                width = scrollWidth < offsetWidth ? W.width() : scrollWidth;

            } else {
                width = D.width();
            }

            this.overlay.width(width).height(D.height());
        },

        // This is where we can manipulate DOM, because later it would cause iframes to reload
        onReady: function (opts, obj) {
            $('.fancybox-overlay').stop(true, true);

            if (!this.overlay) {
                $.extend(this, {
                    overlay: $('<div class="fancybox-overlay"></div>').appendTo(obj.parent),
                    margin: $('body').css('margin-right'),
                    el: document.all && !document.querySelector ? $('html') : $('body')
                });
            }

            if (obj.fixed && !isTouch) {
                this.overlay.addClass('fancybox-overlay-fixed');

                if (obj.autoCenter) {
                    this.overlay.append(obj.wrap);

                    obj.locked = true;
                }
            }

            if (opts.showEarly === true) {
                this.beforeShow.apply(this, arguments);
            }
        },

        beforeShow: function (opts, obj) {
            var overlay = this.overlay.unbind('.fb').width('auto').height('auto').css(opts.css);

            if (opts.closeClick) {
                overlay.bind('click.fb', function (e) {
                    if ($(e.target).hasClass('fancybox-overlay')) {
                        F.close();
                    }
                });
            }

            if (obj.fixed && !isTouch) {
                if (obj.locked) {
                    this.el.addClass('fancybox-lock');

                    if (D.height() > W.height()) {
                        $('body').css('margin-right', getScalar(this.margin) + obj.scrollbarWidth);
                    }
                }

            } else {
                this.update();
            }

            overlay.show();
        },

        onUpdate: function (opts, obj) {
            if (!obj.fixed || isTouch) {
                this.update();
            }
        },

        afterClose: function (opts) {
            var that = this,
				speed = opts.speedOut || 0;

            // Older IE show black background if animating transparent element having filters
            if ($.browser.msie && getScalar($.browser.version) < 9) {
                speed = 0;
            }

            // Remove overlay if exists and fancyBox is not opening
            // (e.g., it is not being open using afterClose callback)
            if (that.overlay && !F.isActive) {
                that.overlay.fadeOut(speed || 0, function () {
                    $('body').css('margin-right', that.margin);

                    that.el.removeClass('fancybox-lock');

                    that.overlay.remove();

                    that.overlay = null;
                });
            }
        }
    };

    /*
	 *	Title helper
	 */

    F.helpers.title = {
        beforeShow: function (opts) {
            var text = F.current.title,
				type = opts.type,
				title,
				target;

            if (!isString(text) || $.trim(text) === '') {
                return;
            }

            title = $('<div class="fancybox-title fancybox-title-' + type + '-wrap">' + text + '</div>');

            switch (type) {
                case 'inside':
                    target = F.skin;
                    break;

                case 'outside':
                    target = F.wrap;
                    break;

                case 'over':
                    target = F.inner;
                    break;

                default: // 'float'
                    target = F.skin;

                    title
						.appendTo('body')
						.width(title.width()) //This helps for some browsers
						.wrapInner('<span class="child"></span>');

                    //Increase bottom margin so this title will also fit into viewport
                    F.current.margin[2] += Math.abs(getScalar(title.css('margin-bottom')));
                    break;
            }

            if (opts.position === 'top') {
                title.prependTo(target);

            } else {
                title.appendTo(target);
            }
        }
    };

    // jQuery plugin initialization
    $.fn.fancybox = function (options) {
        var index,
			that = $(this),
			selector = this.selector || '',
			run = function (e) {
			    var what = $(this).blur(), idx = index, relType, relVal;

			    if (!(e.ctrlKey || e.altKey || e.shiftKey || e.metaKey) && !what.is('.fancybox-wrap')) {
			        relType = options.groupAttr || 'data-fancybox-group';
			        relVal = what.attr(relType);

			        if (!relVal) {
			            relType = 'rel';
			            relVal = what.get(0)[relType];
			        }

			        if (relVal && relVal !== '' && relVal !== 'nofollow') {
			            what = selector.length ? $(selector) : that;
			            what = what.filter('[' + relType + '="' + relVal + '"]');
			            idx = what.index(this);
			        }

			        options.index = idx;

			        // Stop an event from bubbling if everything is fine
			        if (F.open(what, options) !== false) {
			            e.preventDefault();
			        }
			    }
			};

        options = options || {};
        index = options.index || 0;

        if (!selector || options.live === false) {
            that.unbind('click.fb-start').bind('click.fb-start', run);
        } else {
            D.undelegate(selector, 'click.fb-start').delegate(selector + ":not('.fancybox-item, .fancybox-nav')", 'click.fb-start', run);
        }

        return this;
    };

    if (!$.scrollbarWidth) {
        // http://benalman.com/projects/jquery-misc-plugins/#scrollbarwidth
        $.scrollbarWidth = function () {
            var parent, child, width;
            parent = $('<div style="width:50px;height:50px;overflow:auto"><div/></div>').appendTo('body');
            child = parent.children();
            width = child.innerWidth() - child.height(99).innerWidth();
            parent.remove();

            return width;
        };
    }

    // Tests that need a body at doc ready
    D.ready(function () {
        $.extend(F.defaults, {
            scrollbarWidth: $.scrollbarWidth(),
            fixed: $.support.fixedPosition,
            parent: $('body')
        });
    });

}(window, document, jQuery));

/*!
* Buttons helper for fancyBox
* version: 1.0.3
* @requires fancyBox v2.0 or later
*
* Usage:
*     $(".fancybox").fancybox({
*         helpers : {
*             buttons: {
*                 position : 'top'
*             }
*         }
*     });
*
* Options:
*     tpl - HTML template
*     position - 'top' or 'bottom'
*
*/
(function ($) {
    //Shortcut for fancyBox object
    var F = $.fancybox;

    //Add helper object
    F.helpers.buttons = {
        tpl: '<div id="fancybox-buttons"><ul><li><a class="btnPrev" title="Previous" href="javascript:;"></a></li><li><a class="btnPlay" title="Start slideshow" href="javascript:;"></a></li><li><a class="btnNext" title="Next" href="javascript:;"></a></li><li><a class="btnToggle" title="Toggle size" href="javascript:;"></a></li><li><a class="btnClose" title="Close" href="javascript:jQuery.fancybox.close();"></a></li></ul></div>',
        list: null,
        buttons: null,

        beforeLoad: function (opts, obj) {
            //Remove self if gallery do not have at least two items

            if (opts.skipSingle && obj.group.length < 2) {
                obj.helpers.buttons = false;
                obj.closeBtn = true;

                return;
            }

            //Increase top margin to give space for buttons
            obj.margin[opts.position === 'bottom' ? 2 : 0] += 30;
        },

        onPlayStart: function () {
            if (this.buttons) {
                this.buttons.play.attr('title', 'Pause slideshow').addClass('btnPlayOn');
            }
        },

        onPlayEnd: function () {
            if (this.buttons) {
                this.buttons.play.attr('title', 'Start slideshow').removeClass('btnPlayOn');
            }
        },

        afterShow: function (opts, obj) {
            var buttons = this.buttons;

            if (!buttons) {
                this.list = $(opts.tpl || this.tpl).addClass(opts.position || 'top').appendTo('body');

                buttons = {
                    prev: this.list.find('.btnPrev').click(F.prev),
                    next: this.list.find('.btnNext').click(F.next),
                    play: this.list.find('.btnPlay').click(F.play),
                    toggle: this.list.find('.btnToggle').click(F.toggle)
                }
            }

            //Prev
            if (obj.index > 0 || obj.loop) {
                buttons.prev.removeClass('btnDisabled');
            } else {
                buttons.prev.addClass('btnDisabled');
            }

            //Next / Play
            if (obj.loop || obj.index < obj.group.length - 1) {
                buttons.next.removeClass('btnDisabled');
                buttons.play.removeClass('btnDisabled');

            } else {
                buttons.next.addClass('btnDisabled');
                buttons.play.addClass('btnDisabled');
            }

            this.buttons = buttons;

            this.onUpdate(opts, obj);
        },

        onUpdate: function (opts, obj) {
            var toggle;

            if (!this.buttons) {
                return;
            }

            toggle = this.buttons.toggle.removeClass('btnDisabled btnToggleOn');

            //Size toggle button
            if (obj.canShrink) {
                toggle.addClass('btnToggleOn');

            } else if (!obj.canExpand) {
                toggle.addClass('btnDisabled');
            }
        },

        beforeClose: function () {
            if (this.list) {
                this.list.remove();
            }

            this.list = null;
            this.buttons = null;
        }
    };

}(jQuery));

/**
 * @preserve SelectNav.js (v. 0.1)
 * Converts your <ul>/<ol> navigation into a dropdown list for small screens
 * https://github.com/lukaszfiszer/selectnav.js
 */

window.selectnav = (function () {

    "use strict";

    var selectnav = function (element, options) {

        element = document.getElementById(element);

        // return immediately if element doesn't exist
        if (!element) {
            return;
        }

        // return immediately if element is not a list
        if (!islist(element)) {
            return;
        }

        // add a js class to <html> tag
        document.documentElement.className += " js";

        // retreive options and set defaults
        var o = options || {},

          activeclass = o.activeclass || 'active',
          autoselect = typeof (o.autoselect) === "boolean" ? o.autoselect : true,
          nested = typeof (o.nested) === "boolean" ? o.nested : true,
          indent = o.indent || "→",
          label = o.label || "- Navigation -",

          // helper variables
          level = 0,
          selected = " selected ";

        // insert the freshly created dropdown navigation after the existing navigation
        element.insertAdjacentHTML('afterend', parselist(element));

        var nav = document.getElementById(id());

        // autoforward on click
        if (nav.addEventListener) {
            nav.addEventListener('change', goTo);
        }
        if (nav.attachEvent) {
            nav.attachEvent('onchange', goTo);
        }

        return nav;

        function goTo(e) {

            // Crossbrowser issues - http://www.quirksmode.org/js/events_properties.html
            var targ;
            if (!e) e = window.event;
            if (e.target) targ = e.target;
            else if (e.srcElement) targ = e.srcElement;
            if (targ.nodeType === 3) // defeat Safari bug
                targ = targ.parentNode;

            if (targ.value) window.location.href = targ.value;
        }

        function islist(list) {
            var n = list.nodeName.toLowerCase();
            return (n === 'ul' || n === 'ol');
        }

        function id(nextId) {
            for (var j = 1; document.getElementById('selectnav' + j) ; j++);
            return (nextId) ? 'selectnav' + j : 'selectnav' + (j - 1);
        }

        function parselist(list) {

            // go one level down
            level++;

            var length = list.children.length,
              html = '',
              prefix = '',
              k = level - 1
            ;

            // return immediately if has no children
            if (!length) {
                return;
            }

            if (k) {
                while (k--) {
                    prefix += indent;
                }
                prefix += " ";
            }

            for (var i = 0; i < length; i++) {

                var link = list.children[i].children[0];
                if (typeof (link) !== 'undefined') {
                    var text = link.innerText || link.textContent;
                    var isselected = '';

                    if (activeclass) {
                        isselected = link.className.search(activeclass) !== -1 || link.parentElement.className.search(activeclass) !== -1 ? selected : '';
                    }

                    if (autoselect && !isselected) {
                        isselected = link.href === document.URL ? selected : '';
                    }

                    html += '<option value="' + link.href + '" ' + isselected + '>' + prefix + text + '</option>';

                    if (nested) {
                        var subElement = list.children[i].children[1];
                        if (subElement && islist(subElement)) {
                            html += parselist(subElement);
                        }
                    }
                }
            }

            // adds label
            if (level === 1 && label) {
                html = '<option value="">' + label + '</option>' + html;
            }

            // add <select> tag to the top level of the list
            if (level === 1) {
                html = '<select class="selectnav" id="' + id(true) + '">' + html + '</select>';
            }

            // go 1 level up
            level--;

            return html;
        }

    };

    return function (element, options) {
        selectnav(element, options);
    };


})();


/*!
 * jQuery UI Effects 1.8.21
 *
 * Copyright 2012, AUTHORS.txt (http://jqueryui.com/about)
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * http://jquery.org/license
 *
 * http://docs.jquery.com/UI/Effects/
 */
; jQuery.effects || (function ($, undefined) {

    $.effects = {};



    /******************************************************************************/
    /****************************** COLOR ANIMATIONS ******************************/
    /******************************************************************************/

    // override the animation for color styles
    $.each(['backgroundColor', 'borderBottomColor', 'borderLeftColor',
        'borderRightColor', 'borderTopColor', 'borderColor', 'color', 'outlineColor'],
    function (i, attr) {
        $.fx.step[attr] = function (fx) {
            if (!fx.colorInit) {
                fx.start = getColor(fx.elem, attr);
                fx.end = getRGB(fx.end);
                fx.colorInit = true;
            }

            fx.elem.style[attr] = 'rgb(' +
                Math.max(Math.min(parseInt((fx.pos * (fx.end[0] - fx.start[0])) + fx.start[0], 10), 255), 0) + ',' +
                Math.max(Math.min(parseInt((fx.pos * (fx.end[1] - fx.start[1])) + fx.start[1], 10), 255), 0) + ',' +
                Math.max(Math.min(parseInt((fx.pos * (fx.end[2] - fx.start[2])) + fx.start[2], 10), 255), 0) + ')';
        };
    });

    // Color Conversion functions from highlightFade
    // By Blair Mitchelmore
    // http://jquery.offput.ca/highlightFade/

    // Parse strings looking for color tuples [255,255,255]
    function getRGB(color) {
        var result;

        // Check if we're already dealing with an array of colors
        if (color && color.constructor == Array && color.length == 3)
            return color;

        // Look for rgb(num,num,num)
        if (result = /rgb\(\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*\)/.exec(color))
            return [parseInt(result[1], 10), parseInt(result[2], 10), parseInt(result[3], 10)];

        // Look for rgb(num%,num%,num%)
        if (result = /rgb\(\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\%\s*,\s*([0-9]+(?:\.[0-9]+)?)\%\s*\)/.exec(color))
            return [parseFloat(result[1]) * 2.55, parseFloat(result[2]) * 2.55, parseFloat(result[3]) * 2.55];

        // Look for #a0b1c2
        if (result = /#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})/.exec(color))
            return [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16)];

        // Look for #fff
        if (result = /#([a-fA-F0-9])([a-fA-F0-9])([a-fA-F0-9])/.exec(color))
            return [parseInt(result[1] + result[1], 16), parseInt(result[2] + result[2], 16), parseInt(result[3] + result[3], 16)];

        // Look for rgba(0, 0, 0, 0) == transparent in Safari 3
        if (result = /rgba\(0, 0, 0, 0\)/.exec(color))
            return colors['transparent'];

        // Otherwise, we're most likely dealing with a named color
        return colors[$.trim(color).toLowerCase()];
    }

    function getColor(elem, attr) {
        var color;

        do {
            color = $.curCSS(elem, attr);

            // Keep going until we find an element that has color, or we hit the body
            if (color != '' && color != 'transparent' || $.nodeName(elem, "body"))
                break;

            attr = "backgroundColor";
        } while (elem = elem.parentNode);

        return getRGB(color);
    };

    // Some named colors to work with
    // From Interface by Stefan Petre
    // http://interface.eyecon.ro/

    var colors = {
        aqua: [0, 255, 255],
        azure: [240, 255, 255],
        beige: [245, 245, 220],
        black: [0, 0, 0],
        blue: [0, 0, 255],
        brown: [165, 42, 42],
        cyan: [0, 255, 255],
        darkblue: [0, 0, 139],
        darkcyan: [0, 139, 139],
        darkgrey: [169, 169, 169],
        darkgreen: [0, 100, 0],
        darkkhaki: [189, 183, 107],
        darkmagenta: [139, 0, 139],
        darkolivegreen: [85, 107, 47],
        darkorange: [255, 140, 0],
        darkorchid: [153, 50, 204],
        darkred: [139, 0, 0],
        darksalmon: [233, 150, 122],
        darkviolet: [148, 0, 211],
        fuchsia: [255, 0, 255],
        gold: [255, 215, 0],
        green: [0, 128, 0],
        indigo: [75, 0, 130],
        khaki: [240, 230, 140],
        lightblue: [173, 216, 230],
        lightcyan: [224, 255, 255],
        lightgreen: [144, 238, 144],
        lightgrey: [211, 211, 211],
        lightpink: [255, 182, 193],
        lightyellow: [255, 255, 224],
        lime: [0, 255, 0],
        magenta: [255, 0, 255],
        maroon: [128, 0, 0],
        navy: [0, 0, 128],
        olive: [128, 128, 0],
        orange: [255, 165, 0],
        pink: [255, 192, 203],
        purple: [128, 0, 128],
        violet: [128, 0, 128],
        red: [255, 0, 0],
        silver: [192, 192, 192],
        white: [255, 255, 255],
        yellow: [255, 255, 0],
        transparent: [255, 255, 255]
    };



    /******************************************************************************/
    /****************************** CLASS ANIMATIONS ******************************/
    /******************************************************************************/

    var classAnimationActions = ['add', 'remove', 'toggle'],
        shorthandStyles = {
            border: 1,
            borderBottom: 1,
            borderColor: 1,
            borderLeft: 1,
            borderRight: 1,
            borderTop: 1,
            borderWidth: 1,
            margin: 1,
            padding: 1
        };

    function getElementStyles() {
        var style = document.defaultView
                ? document.defaultView.getComputedStyle(this, null)
                : this.currentStyle,
            newStyle = {},
            key,
            camelCase;

        // webkit enumerates style porperties
        if (style && style.length && style[0] && style[style[0]]) {
            var len = style.length;
            while (len--) {
                key = style[len];
                if (typeof style[key] == 'string') {
                    camelCase = key.replace(/\-(\w)/g, function (all, letter) {
                        return letter.toUpperCase();
                    });
                    newStyle[camelCase] = style[key];
                }
            }
        } else {
            for (key in style) {
                if (typeof style[key] === 'string') {
                    newStyle[key] = style[key];
                }
            }
        }

        return newStyle;
    }

    function filterStyles(styles) {
        var name, value;
        for (name in styles) {
            value = styles[name];
            if (
                // ignore null and undefined values
                value == null ||
                // ignore functions (when does this occur?)
                $.isFunction(value) ||
                // shorthand styles that need to be expanded
                name in shorthandStyles ||
                // ignore scrollbars (break in IE)
                (/scrollbar/).test(name) ||

                // only colors or values that can be converted to numbers
                (!(/color/i).test(name) && isNaN(parseFloat(value)))
            ) {
                delete styles[name];
            }
        }

        return styles;
    }

    function styleDifference(oldStyle, newStyle) {
        var diff = { _: 0 }, // http://dev.jquery.com/ticket/5459
            name;

        for (name in newStyle) {
            if (oldStyle[name] != newStyle[name]) {
                diff[name] = newStyle[name];
            }
        }

        return diff;
    }

    $.effects.animateClass = function (value, duration, easing, callback) {
        if ($.isFunction(easing)) {
            callback = easing;
            easing = null;
        }

        return this.queue(function () {
            var that = $(this),
                originalStyleAttr = that.attr('style') || ' ',
                originalStyle = filterStyles(getElementStyles.call(this)),
                newStyle,
                className = that.attr('class') || "";

            $.each(classAnimationActions, function (i, action) {
                if (value[action]) {
                    that[action + 'Class'](value[action]);
                }
            });
            newStyle = filterStyles(getElementStyles.call(this));
            that.attr('class', className);

            that.animate(styleDifference(originalStyle, newStyle), {
                queue: false,
                duration: duration,
                easing: easing,
                complete: function () {
                    $.each(classAnimationActions, function (i, action) {
                        if (value[action]) { that[action + 'Class'](value[action]); }
                    });
                    // work around bug in IE by clearing the cssText before setting it
                    if (typeof that.attr('style') == 'object') {
                        that.attr('style').cssText = '';
                        that.attr('style').cssText = originalStyleAttr;
                    } else {
                        that.attr('style', originalStyleAttr);
                    }
                    if (callback) { callback.apply(this, arguments); }
                    $.dequeue(this);
                }
            });
        });
    };

    $.fn.extend({
        _addClass: $.fn.addClass,
        addClass: function (classNames, speed, easing, callback) {
            return speed ? $.effects.animateClass.apply(this, [{ add: classNames }, speed, easing, callback]) : this._addClass(classNames);
        },

        _removeClass: $.fn.removeClass,
        removeClass: function (classNames, speed, easing, callback) {
            return speed ? $.effects.animateClass.apply(this, [{ remove: classNames }, speed, easing, callback]) : this._removeClass(classNames);
        },

        _toggleClass: $.fn.toggleClass,
        toggleClass: function (classNames, force, speed, easing, callback) {
            if (typeof force == "boolean" || force === undefined) {
                if (!speed) {
                    // without speed parameter;
                    return this._toggleClass(classNames, force);
                } else {
                    return $.effects.animateClass.apply(this, [(force ? { add: classNames } : { remove: classNames }), speed, easing, callback]);
                }
            } else {
                // without switch parameter;
                return $.effects.animateClass.apply(this, [{ toggle: classNames }, force, speed, easing]);
            }
        },

        switchClass: function (remove, add, speed, easing, callback) {
            return $.effects.animateClass.apply(this, [{ add: add, remove: remove }, speed, easing, callback]);
        }
    });
    /******************************************************************************/
    /*********************************** EFFECTS **********************************/
    /******************************************************************************/

    $.extend($.effects, {
        version: "1.8.21",

        // Saves a set of properties in a data storage
        save: function (element, set) {
            for (var i = 0; i < set.length; i++) {
                if (set[i] !== null) element.data("ec.storage." + set[i], element[0].style[set[i]]);
            }
        },

        // Restores a set of previously saved properties from a data storage
        restore: function (element, set) {
            for (var i = 0; i < set.length; i++) {
                if (set[i] !== null) element.css(set[i], element.data("ec.storage." + set[i]));
            }
        },

        setMode: function (el, mode) {
            if (mode == 'toggle') mode = el.is(':hidden') ? 'show' : 'hide'; // Set for toggle
            return mode;
        },

        getBaseline: function (origin, original) { // Translates a [top,left] array into a baseline value
            // this should be a little more flexible in the future to handle a string & hash
            var y, x;
            switch (origin[0]) {
                case 'top': y = 0; break;
                case 'middle': y = 0.5; break;
                case 'bottom': y = 1; break;
                default: y = origin[0] / original.height;
            };
            switch (origin[1]) {
                case 'left': x = 0; break;
                case 'center': x = 0.5; break;
                case 'right': x = 1; break;
                default: x = origin[1] / original.width;
            };
            return { x: x, y: y };
        },

        // Wraps the element around a wrapper that copies position properties
        createWrapper: function (element) {

            // if the element is already wrapped, return it
            if (element.parent().is('.ui-effects-wrapper')) {
                return element.parent();
            }

            // wrap the element
            var props = {
                width: element.outerWidth(true),
                height: element.outerHeight(true),
                'float': element.css('float')
            },
                wrapper = $('<div></div>')
                    .addClass('ui-effects-wrapper')
                    .css({
                        fontSize: '100%',
                        background: 'transparent',
                        border: 'none',
                        margin: 0,
                        padding: 0
                    }),
                active = document.activeElement;

            // support: Firefox
            // Firefox incorrectly exposes anonymous content
            // https://bugzilla.mozilla.org/show_bug.cgi?id=561664
            try {
                active.id;
            } catch (e) {
                active = document.body;
            }

            element.wrap(wrapper);

            // Fixes #7595 - Elements lose focus when wrapped.
            if (element[0] === active || $.contains(element[0], active)) {
                $(active).focus();
            }

            wrapper = element.parent(); //Hotfix for jQuery 1.4 since some change in wrap() seems to actually loose the reference to the wrapped element

            // transfer positioning properties to the wrapper
            if (element.css('position') == 'static') {
                wrapper.css({ position: 'relative' });
                element.css({ position: 'relative' });
            } else {
                $.extend(props, {
                    position: element.css('position'),
                    zIndex: element.css('z-index')
                });
                $.each(['top', 'left', 'bottom', 'right'], function (i, pos) {
                    props[pos] = element.css(pos);
                    if (isNaN(parseInt(props[pos], 10))) {
                        props[pos] = 'auto';
                    }
                });
                element.css({ position: 'relative', top: 0, left: 0, right: 'auto', bottom: 'auto' });
            }

            return wrapper.css(props).show();
        },

        removeWrapper: function (element) {
            var parent,
                active = document.activeElement;

            if (element.parent().is('.ui-effects-wrapper')) {
                parent = element.parent().replaceWith(element);
                // Fixes #7595 - Elements lose focus when wrapped.
                if (element[0] === active || $.contains(element[0], active)) {
                    $(active).focus();
                }
                return parent;
            }

            return element;
        },

        setTransition: function (element, list, factor, value) {
            value = value || {};
            $.each(list, function (i, x) {
                var unit = element.cssUnit(x);
                if (unit[0] > 0) value[x] = unit[0] * factor + unit[1];
            });
            return value;
        }
    });


    function _normalizeArguments(effect, options, speed, callback) {
        // shift params for method overloading
        if (typeof effect == 'object') {
            callback = options;
            speed = null;
            options = effect;
            effect = options.effect;
        }
        if ($.isFunction(options)) {
            callback = options;
            speed = null;
            options = {};
        }
        if (typeof options == 'number' || $.fx.speeds[options]) {
            callback = speed;
            speed = options;
            options = {};
        }
        if ($.isFunction(speed)) {
            callback = speed;
            speed = null;
        }

        options = options || {};

        speed = speed || options.duration;
        speed = $.fx.off ? 0 : typeof speed == 'number'
            ? speed : speed in $.fx.speeds ? $.fx.speeds[speed] : $.fx.speeds._default;

        callback = callback || options.complete;

        return [effect, options, speed, callback];
    }

    function standardSpeed(speed) {
        // valid standard speeds
        if (!speed || typeof speed === "number" || $.fx.speeds[speed]) {
            return true;
        }

        // invalid strings - treat as "normal" speed
        if (typeof speed === "string" && !$.effects[speed]) {
            return true;
        }

        return false;
    }

    $.fn.extend({
        effect: function (effect, options, speed, callback) {
            var args = _normalizeArguments.apply(this, arguments),
                // TODO: make effects take actual parameters instead of a hash
                args2 = {
                    options: args[1],
                    duration: args[2],
                    callback: args[3]
                },
                mode = args2.options.mode,
                effectMethod = $.effects[effect];

            if ($.fx.off || !effectMethod) {
                // delegate to the original method (e.g., .show()) if possible
                if (mode) {
                    return this[mode](args2.duration, args2.callback);
                } else {
                    return this.each(function () {
                        if (args2.callback) {
                            args2.callback.call(this);
                        }
                    });
                }
            }

            return effectMethod.call(this, args2);
        },

        _show: $.fn.show,
        show: function (speed) {
            if (standardSpeed(speed)) {
                return this._show.apply(this, arguments);
            } else {
                var args = _normalizeArguments.apply(this, arguments);
                args[1].mode = 'show';
                return this.effect.apply(this, args);
            }
        },

        _hide: $.fn.hide,
        hide: function (speed) {
            if (standardSpeed(speed)) {
                return this._hide.apply(this, arguments);
            } else {
                var args = _normalizeArguments.apply(this, arguments);
                args[1].mode = 'hide';
                return this.effect.apply(this, args);
            }
        },

        // jQuery core overloads toggle and creates _toggle
        __toggle: $.fn.toggle,
        toggle: function (speed) {
            if (standardSpeed(speed) || typeof speed === "boolean" || $.isFunction(speed)) {
                return this.__toggle.apply(this, arguments);
            } else {
                var args = _normalizeArguments.apply(this, arguments);
                args[1].mode = 'toggle';
                return this.effect.apply(this, args);
            }
        },

        // helper functions
        cssUnit: function (key) {
            var style = this.css(key), val = [];
            $.each(['em', 'px', '%', 'pt'], function (i, unit) {
                if (style.indexOf(unit) > 0)
                    val = [parseFloat(style), unit];
            });
            return val;
        }
    });



    /******************************************************************************/
    /*********************************** EASING ***********************************/
    /******************************************************************************/

    /*
     * jQuery Easing v1.3 - http://gsgd.co.uk/sandbox/jquery/easing/
     *
     * Uses the built in easing capabilities added In jQuery 1.1
     * to offer multiple easing options
     *
     * TERMS OF USE - jQuery Easing
     *
     * Open source under the BSD License.
     *
     * Copyright 2008 George McGinley Smith
     * All rights reserved.
     *
     * Redistribution and use in source and binary forms, with or without modification,
     * are permitted provided that the following conditions are met:
     *
     * Redistributions of source code must retain the above copyright notice, this list of
     * conditions and the following disclaimer.
     * Redistributions in binary form must reproduce the above copyright notice, this list
     * of conditions and the following disclaimer in the documentation and/or other materials
     * provided with the distribution.
     *
     * Neither the name of the author nor the names of contributors may be used to endorse
     * or promote products derived from this software without specific prior written permission.
     *
     * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
     * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
     * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
     * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
     * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
     * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
     * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
     * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
     * OF THE POSSIBILITY OF SUCH DAMAGE.
     *
    */

    // t: current time, b: begInnIng value, c: change In value, d: duration
    $.easing.jswing = $.easing.swing;

    $.extend($.easing,
    {
        def: 'easeOutQuad',
        swing: function (x, t, b, c, d) {
            //alert($.easing.default);
            return $.easing[$.easing.def](x, t, b, c, d);
        },
        easeInQuad: function (x, t, b, c, d) {
            return c * (t /= d) * t + b;
        },
        easeOutQuad: function (x, t, b, c, d) {
            return -c * (t /= d) * (t - 2) + b;
        },
        easeInOutQuad: function (x, t, b, c, d) {
            if ((t /= d / 2) < 1) return c / 2 * t * t + b;
            return -c / 2 * ((--t) * (t - 2) - 1) + b;
        },
        easeInCubic: function (x, t, b, c, d) {
            return c * (t /= d) * t * t + b;
        },
        easeOutCubic: function (x, t, b, c, d) {
            return c * ((t = t / d - 1) * t * t + 1) + b;
        },
        easeInOutCubic: function (x, t, b, c, d) {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t + 2) + b;
        },
        easeInQuart: function (x, t, b, c, d) {
            return c * (t /= d) * t * t * t + b;
        },
        easeOutQuart: function (x, t, b, c, d) {
            return -c * ((t = t / d - 1) * t * t * t - 1) + b;
        },
        easeInOutQuart: function (x, t, b, c, d) {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
            return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
        },
        easeInQuint: function (x, t, b, c, d) {
            return c * (t /= d) * t * t * t * t + b;
        },
        easeOutQuint: function (x, t, b, c, d) {
            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        },
        easeInOutQuint: function (x, t, b, c, d) {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
        },
        easeInSine: function (x, t, b, c, d) {
            return -c * Math.cos(t / d * (Math.PI / 2)) + c + b;
        },
        easeOutSine: function (x, t, b, c, d) {
            return c * Math.sin(t / d * (Math.PI / 2)) + b;
        },
        easeInOutSine: function (x, t, b, c, d) {
            return -c / 2 * (Math.cos(Math.PI * t / d) - 1) + b;
        },
        easeInExpo: function (x, t, b, c, d) {
            return (t == 0) ? b : c * Math.pow(2, 10 * (t / d - 1)) + b;
        },
        easeOutExpo: function (x, t, b, c, d) {
            return (t == d) ? b + c : c * (-Math.pow(2, -10 * t / d) + 1) + b;
        },
        easeInOutExpo: function (x, t, b, c, d) {
            if (t == 0) return b;
            if (t == d) return b + c;
            if ((t /= d / 2) < 1) return c / 2 * Math.pow(2, 10 * (t - 1)) + b;
            return c / 2 * (-Math.pow(2, -10 * --t) + 2) + b;
        },
        easeInCirc: function (x, t, b, c, d) {
            return -c * (Math.sqrt(1 - (t /= d) * t) - 1) + b;
        },
        easeOutCirc: function (x, t, b, c, d) {
            return c * Math.sqrt(1 - (t = t / d - 1) * t) + b;
        },
        easeInOutCirc: function (x, t, b, c, d) {
            if ((t /= d / 2) < 1) return -c / 2 * (Math.sqrt(1 - t * t) - 1) + b;
            return c / 2 * (Math.sqrt(1 - (t -= 2) * t) + 1) + b;
        },
        easeInElastic: function (x, t, b, c, d) {
            var s = 1.70158; var p = 0; var a = c;
            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (!p) p = d * .3;
            if (a < Math.abs(c)) { a = c; var s = p / 4; }
            else var s = p / (2 * Math.PI) * Math.asin(c / a);
            return -(a * Math.pow(2, 10 * (t -= 1)) * Math.sin((t * d - s) * (2 * Math.PI) / p)) + b;
        },
        easeOutElastic: function (x, t, b, c, d) {
            var s = 1.70158; var p = 0; var a = c;
            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (!p) p = d * .3;
            if (a < Math.abs(c)) { a = c; var s = p / 4; }
            else var s = p / (2 * Math.PI) * Math.asin(c / a);
            return a * Math.pow(2, -10 * t) * Math.sin((t * d - s) * (2 * Math.PI) / p) + c + b;
        },
        easeInOutElastic: function (x, t, b, c, d) {
            var s = 1.70158; var p = 0; var a = c;
            if (t == 0) return b; if ((t /= d / 2) == 2) return b + c; if (!p) p = d * (.3 * 1.5);
            if (a < Math.abs(c)) { a = c; var s = p / 4; }
            else var s = p / (2 * Math.PI) * Math.asin(c / a);
            if (t < 1) return -.5 * (a * Math.pow(2, 10 * (t -= 1)) * Math.sin((t * d - s) * (2 * Math.PI) / p)) + b;
            return a * Math.pow(2, -10 * (t -= 1)) * Math.sin((t * d - s) * (2 * Math.PI) / p) * .5 + c + b;
        },
        easeInBack: function (x, t, b, c, d, s) {
            if (s == undefined) s = 1.70158;
            return c * (t /= d) * t * ((s + 1) * t - s) + b;
        },
        easeOutBack: function (x, t, b, c, d, s) {
            if (s == undefined) s = 1.70158;
            return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
        },
        easeInOutBack: function (x, t, b, c, d, s) {
            if (s == undefined) s = 1.70158;
            if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;
            return c / 2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
        },
        easeInBounce: function (x, t, b, c, d) {
            return c - $.easing.easeOutBounce(x, d - t, 0, c, d) + b;
        },
        easeOutBounce: function (x, t, b, c, d) {
            if ((t /= d) < (1 / 2.75)) {
                return c * (7.5625 * t * t) + b;
            } else if (t < (2 / 2.75)) {
                return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
            } else if (t < (2.5 / 2.75)) {
                return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
            } else {
                return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
            }
        },
        easeInOutBounce: function (x, t, b, c, d) {
            if (t < d / 2) return $.easing.easeInBounce(x, t * 2, 0, c, d) * .5 + b;
            return $.easing.easeOutBounce(x, t * 2 - d, 0, c, d) * .5 + c * .5 + b;
        }
    });

    /*
     *
     * TERMS OF USE - EASING EQUATIONS
     *
     * Open source under the BSD License.
     *
     * Copyright 2001 Robert Penner
     * All rights reserved.
     *
     * Redistribution and use in source and binary forms, with or without modification,
     * are permitted provided that the following conditions are met:
     *
     * Redistributions of source code must retain the above copyright notice, this list of
     * conditions and the following disclaimer.
     * Redistributions in binary form must reproduce the above copyright notice, this list
     * of conditions and the following disclaimer in the documentation and/or other materials
     * provided with the distribution.
     *
     * Neither the name of the author nor the names of contributors may be used to endorse
     * or promote products derived from this software without specific prior written permission.
     *
     * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
     * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
     * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
     * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
     * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
     * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
     * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
     * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
     * OF THE POSSIBILITY OF SUCH DAMAGE.
     *
     */

})(jQuery);


/**
 * Isotope v1.5.19
 * An exquisite jQuery plugin for magical layouts
 * http://isotope.metafizzy.co
 *
 * Commercial use requires one-time license fee
 * http://metafizzy.co/#licenses
 *
 * Copyright 2012 David DeSandro / Metafizzy
 */
(function (a, b, c) { "use strict"; var d = a.document, e = a.Modernizr, f = function (a) { return a.charAt(0).toUpperCase() + a.slice(1) }, g = "Moz Webkit O Ms".split(" "), h = function (a) { var b = d.documentElement.style, c; if (typeof b[a] == "string") return a; a = f(a); for (var e = 0, h = g.length; e < h; e++) { c = g[e] + a; if (typeof b[c] == "string") return c } }, i = h("transform"), j = h("transitionProperty"), k = { csstransforms: function () { return !!i }, csstransforms3d: function () { var a = !!h("perspective"); if (a) { var c = " -o- -moz- -ms- -webkit- -khtml- ".split(" "), d = "@media (" + c.join("transform-3d),(") + "modernizr)", e = b("<style>" + d + "{#modernizr{height:3px}}" + "</style>").appendTo("head"), f = b('<div id="modernizr" />').appendTo("html"); a = f.height() === 3, f.remove(), e.remove() } return a }, csstransitions: function () { return !!j } }, l; if (e) for (l in k) e.hasOwnProperty(l) || e.addTest(l, k[l]); else { e = a.Modernizr = { _version: "1.6ish: miniModernizr for Isotope" }; var m = " ", n; for (l in k) n = k[l](), e[l] = n, m += " " + (n ? "" : "no-") + l; b("html").addClass(m) } if (e.csstransforms) { var o = e.csstransforms3d ? { translate: function (a) { return "translate3d(" + a[0] + "px, " + a[1] + "px, 0) " }, scale: function (a) { return "scale3d(" + a + ", " + a + ", 1) " } } : { translate: function (a) { return "translate(" + a[0] + "px, " + a[1] + "px) " }, scale: function (a) { return "scale(" + a + ") " } }, p = function (a, c, d) { var e = b.data(a, "isoTransform") || {}, f = {}, g, h = {}, j; f[c] = d, b.extend(e, f); for (g in e) j = e[g], h[g] = o[g](j); var k = h.translate || "", l = h.scale || "", m = k + l; b.data(a, "isoTransform", e), a.style[i] = m }; b.cssNumber.scale = !0, b.cssHooks.scale = { set: function (a, b) { p(a, "scale", b) }, get: function (a, c) { var d = b.data(a, "isoTransform"); return d && d.scale ? d.scale : 1 } }, b.fx.step.scale = function (a) { b.cssHooks.scale.set(a.elem, a.now + a.unit) }, b.cssNumber.translate = !0, b.cssHooks.translate = { set: function (a, b) { p(a, "translate", b) }, get: function (a, c) { var d = b.data(a, "isoTransform"); return d && d.translate ? d.translate : [0, 0] } } } var q, r; e.csstransitions && (q = { WebkitTransitionProperty: "webkitTransitionEnd", MozTransitionProperty: "transitionend", OTransitionProperty: "oTransitionEnd", transitionProperty: "transitionEnd" }[j], r = h("transitionDuration")); var s = b.event, t; s.special.smartresize = { setup: function () { b(this).bind("resize", s.special.smartresize.handler) }, teardown: function () { b(this).unbind("resize", s.special.smartresize.handler) }, handler: function (a, b) { var c = this, d = arguments; a.type = "smartresize", t && clearTimeout(t), t = setTimeout(function () { jQuery.event.handle.apply(c, d) }, b === "execAsap" ? 0 : 100) } }, b.fn.smartresize = function (a) { return a ? this.bind("smartresize", a) : this.trigger("smartresize", ["execAsap"]) }, b.Isotope = function (a, c, d) { this.element = b(c), this._create(a), this._init(d) }; var u = ["width", "height"], v = b(a); b.Isotope.settings = { resizable: !0, layoutMode: "masonry", containerClass: "isotope", itemClass: "isotope-item", hiddenClass: "isotope-hidden", hiddenStyle: { opacity: 0, scale: .001 }, visibleStyle: { opacity: 1, scale: 1 }, containerStyle: { position: "relative", overflow: "hidden" }, animationEngine: "best-available", animationOptions: { queue: !1, duration: 800 }, sortBy: "original-order", sortAscending: !0, resizesContainer: !0, transformsEnabled: !b.browser.opera, itemPositionDataEnabled: !1 }, b.Isotope.prototype = { _create: function (a) { this.options = b.extend({}, b.Isotope.settings, a), this.styleQueue = [], this.elemCount = 0; var c = this.element[0].style; this.originalStyle = {}; var d = u.slice(0); for (var e in this.options.containerStyle) d.push(e); for (var f = 0, g = d.length; f < g; f++) e = d[f], this.originalStyle[e] = c[e] || ""; this.element.css(this.options.containerStyle), this._updateAnimationEngine(), this._updateUsingTransforms(); var h = { "original-order": function (a, b) { return b.elemCount++, b.elemCount }, random: function () { return Math.random() } }; this.options.getSortData = b.extend(this.options.getSortData, h), this.reloadItems(), this.offset = { left: parseInt(this.element.css("padding-left") || 0, 10), top: parseInt(this.element.css("padding-top") || 0, 10) }; var i = this; setTimeout(function () { i.element.addClass(i.options.containerClass) }, 0), this.options.resizable && v.bind("smartresize.isotope", function () { i.resize() }), this.element.delegate("." + this.options.hiddenClass, "click", function () { return !1 }) }, _getAtoms: function (a) { var b = this.options.itemSelector, c = b ? a.filter(b).add(a.find(b)) : a, d = { position: "absolute" }; return this.usingTransforms && (d.left = 0, d.top = 0), c.css(d).addClass(this.options.itemClass), this.updateSortData(c, !0), c }, _init: function (a) { this.$filteredAtoms = this._filter(this.$allAtoms), this._sort(), this.reLayout(a) }, option: function (a) { if (b.isPlainObject(a)) { this.options = b.extend(!0, this.options, a); var c; for (var d in a) c = "_update" + f(d), this[c] && this[c]() } }, _updateAnimationEngine: function () { var a = this.options.animationEngine.toLowerCase().replace(/[ _\-]/g, ""), b; switch (a) { case "css": case "none": b = !1; break; case "jquery": b = !0; break; default: b = !e.csstransitions } this.isUsingJQueryAnimation = b, this._updateUsingTransforms() }, _updateTransformsEnabled: function () { this._updateUsingTransforms() }, _updateUsingTransforms: function () { var a = this.usingTransforms = this.options.transformsEnabled && e.csstransforms && e.csstransitions && !this.isUsingJQueryAnimation; a || (delete this.options.hiddenStyle.scale, delete this.options.visibleStyle.scale), this.getPositionStyles = a ? this._translate : this._positionAbs }, _filter: function (a) { var b = this.options.filter === "" ? "*" : this.options.filter; if (!b) return a; var c = this.options.hiddenClass, d = "." + c, e = a.filter(d), f = e; if (b !== "*") { f = e.filter(b); var g = a.not(d).not(b).addClass(c); this.styleQueue.push({ $el: g, style: this.options.hiddenStyle }) } return this.styleQueue.push({ $el: f, style: this.options.visibleStyle }), f.removeClass(c), a.filter(b) }, updateSortData: function (a, c) { var d = this, e = this.options.getSortData, f, g; a.each(function () { f = b(this), g = {}; for (var a in e) !c && a === "original-order" ? g[a] = b.data(this, "isotope-sort-data")[a] : g[a] = e[a](f, d); b.data(this, "isotope-sort-data", g) }) }, _sort: function () { var a = this.options.sortBy, b = this._getSorter, c = this.options.sortAscending ? 1 : -1, d = function (d, e) { var f = b(d, a), g = b(e, a); return f === g && a !== "original-order" && (f = b(d, "original-order"), g = b(e, "original-order")), (f > g ? 1 : f < g ? -1 : 0) * c }; this.$filteredAtoms.sort(d) }, _getSorter: function (a, c) { return b.data(a, "isotope-sort-data")[c] }, _translate: function (a, b) { return { translate: [a, b] } }, _positionAbs: function (a, b) { return { left: a, top: b } }, _pushPosition: function (a, b, c) { b = Math.round(b + this.offset.left), c = Math.round(c + this.offset.top); var d = this.getPositionStyles(b, c); this.styleQueue.push({ $el: a, style: d }), this.options.itemPositionDataEnabled && a.data("isotope-item-position", { x: b, y: c }) }, layout: function (a, b) { var c = this.options.layoutMode; this["_" + c + "Layout"](a); if (this.options.resizesContainer) { var d = this["_" + c + "GetContainerSize"](); this.styleQueue.push({ $el: this.element, style: d }) } this._processStyleQueue(a, b), this.isLaidOut = !0 }, _processStyleQueue: function (a, c) { var d = this.isLaidOut ? this.isUsingJQueryAnimation ? "animate" : "css" : "css", f = this.options.animationOptions, g = this.options.onLayout, h, i, j, k; i = function (a, b) { b.$el[d](b.style, f) }; if (this._isInserting && this.isUsingJQueryAnimation) i = function (a, b) { h = b.$el.hasClass("no-transition") ? "css" : d, b.$el[h](b.style, f) }; else if (c || g || f.complete) { var l = !1, m = [c, g, f.complete], n = this; j = !0, k = function () { if (l) return; var b; for (var c = 0, d = m.length; c < d; c++) b = m[c], typeof b == "function" && b.call(n.element, a, n); l = !0 }; if (this.isUsingJQueryAnimation && d === "animate") f.complete = k, j = !1; else if (e.csstransitions) { var o = 0, p = this.styleQueue[0], s = p && p.$el, t; while (!s || !s.length) { t = this.styleQueue[o++]; if (!t) return; s = t.$el } var u = parseFloat(getComputedStyle(s[0])[r]); u > 0 && (i = function (a, b) { b.$el[d](b.style, f).one(q, k) }, j = !1) } } b.each(this.styleQueue, i), j && k(), this.styleQueue = [] }, resize: function () { this["_" + this.options.layoutMode + "ResizeChanged"]() && this.reLayout() }, reLayout: function (a) { this["_" + this.options.layoutMode + "Reset"](), this.layout(this.$filteredAtoms, a) }, addItems: function (a, b) { var c = this._getAtoms(a); this.$allAtoms = this.$allAtoms.add(c), b && b(c) }, insert: function (a, b) { this.element.append(a); var c = this; this.addItems(a, function (a) { var d = c._filter(a); c._addHideAppended(d), c._sort(), c.reLayout(), c._revealAppended(d, b) }) }, appended: function (a, b) { var c = this; this.addItems(a, function (a) { c._addHideAppended(a), c.layout(a), c._revealAppended(a, b) }) }, _addHideAppended: function (a) { this.$filteredAtoms = this.$filteredAtoms.add(a), a.addClass("no-transition"), this._isInserting = !0, this.styleQueue.push({ $el: a, style: this.options.hiddenStyle }) }, _revealAppended: function (a, b) { var c = this; setTimeout(function () { a.removeClass("no-transition"), c.styleQueue.push({ $el: a, style: c.options.visibleStyle }), c._isInserting = !1, c._processStyleQueue(a, b) }, 10) }, reloadItems: function () { this.$allAtoms = this._getAtoms(this.element.children()) }, remove: function (a, b) { var c = this, d = function () { c.$allAtoms = c.$allAtoms.not(a), a.remove(), b && b.call(c.element) }; a.filter(":not(." + this.options.hiddenClass + ")").length ? (this.styleQueue.push({ $el: a, style: this.options.hiddenStyle }), this.$filteredAtoms = this.$filteredAtoms.not(a), this._sort(), this.reLayout(d)) : d() }, shuffle: function (a) { this.updateSortData(this.$allAtoms), this.options.sortBy = "random", this._sort(), this.reLayout(a) }, destroy: function () { var a = this.usingTransforms, b = this.options; this.$allAtoms.removeClass(b.hiddenClass + " " + b.itemClass).each(function () { var b = this.style; b.position = "", b.top = "", b.left = "", b.opacity = "", a && (b[i] = "") }); var c = this.element[0].style; for (var d in this.originalStyle) c[d] = this.originalStyle[d]; this.element.unbind(".isotope").undelegate("." + b.hiddenClass, "click").removeClass(b.containerClass).removeData("isotope"), v.unbind(".isotope") }, _getSegments: function (a) { var b = this.options.layoutMode, c = a ? "rowHeight" : "columnWidth", d = a ? "height" : "width", e = a ? "rows" : "cols", g = this.element[d](), h, i = this.options[b] && this.options[b][c] || this.$filteredAtoms["outer" + f(d)](!0) || g; h = Math.floor(g / i), h = Math.max(h, 1), this[b][e] = h, this[b][c] = i }, _checkIfSegmentsChanged: function (a) { var b = this.options.layoutMode, c = a ? "rows" : "cols", d = this[b][c]; return this._getSegments(a), this[b][c] !== d }, _masonryReset: function () { this.masonry = {}, this._getSegments(); var a = this.masonry.cols; this.masonry.colYs = []; while (a--) this.masonry.colYs.push(0) }, _masonryLayout: function (a) { var c = this, d = c.masonry; a.each(function () { var a = b(this), e = Math.ceil(a.outerWidth(!0) / d.columnWidth); e = Math.min(e, d.cols); if (e === 1) c._masonryPlaceBrick(a, d.colYs); else { var f = d.cols + 1 - e, g = [], h, i; for (i = 0; i < f; i++) h = d.colYs.slice(i, i + e), g[i] = Math.max.apply(Math, h); c._masonryPlaceBrick(a, g) } }) }, _masonryPlaceBrick: function (a, b) { var c = Math.min.apply(Math, b), d = 0; for (var e = 0, f = b.length; e < f; e++) if (b[e] === c) { d = e; break } var g = this.masonry.columnWidth * d, h = c; this._pushPosition(a, g, h); var i = c + a.outerHeight(!0), j = this.masonry.cols + 1 - f; for (e = 0; e < j; e++) this.masonry.colYs[d + e] = i }, _masonryGetContainerSize: function () { var a = Math.max.apply(Math, this.masonry.colYs); return { height: a } }, _masonryResizeChanged: function () { return this._checkIfSegmentsChanged() }, _fitRowsReset: function () { this.fitRows = { x: 0, y: 0, height: 0 } }, _fitRowsLayout: function (a) { var c = this, d = this.element.width(), e = this.fitRows; a.each(function () { var a = b(this), f = a.outerWidth(!0), g = a.outerHeight(!0); e.x !== 0 && f + e.x > d && (e.x = 0, e.y = e.height), c._pushPosition(a, e.x, e.y), e.height = Math.max(e.y + g, e.height), e.x += f }) }, _fitRowsGetContainerSize: function () { return { height: this.fitRows.height } }, _fitRowsResizeChanged: function () { return !0 }, _cellsByRowReset: function () { this.cellsByRow = { index: 0 }, this._getSegments(), this._getSegments(!0) }, _cellsByRowLayout: function (a) { var c = this, d = this.cellsByRow; a.each(function () { var a = b(this), e = d.index % d.cols, f = Math.floor(d.index / d.cols), g = (e + .5) * d.columnWidth - a.outerWidth(!0) / 2, h = (f + .5) * d.rowHeight - a.outerHeight(!0) / 2; c._pushPosition(a, g, h), d.index++ }) }, _cellsByRowGetContainerSize: function () { return { height: Math.ceil(this.$filteredAtoms.length / this.cellsByRow.cols) * this.cellsByRow.rowHeight + this.offset.top } }, _cellsByRowResizeChanged: function () { return this._checkIfSegmentsChanged() }, _straightDownReset: function () { this.straightDown = { y: 0 } }, _straightDownLayout: function (a) { var c = this; a.each(function (a) { var d = b(this); c._pushPosition(d, 0, c.straightDown.y), c.straightDown.y += d.outerHeight(!0) }) }, _straightDownGetContainerSize: function () { return { height: this.straightDown.y } }, _straightDownResizeChanged: function () { return !0 }, _masonryHorizontalReset: function () { this.masonryHorizontal = {}, this._getSegments(!0); var a = this.masonryHorizontal.rows; this.masonryHorizontal.rowXs = []; while (a--) this.masonryHorizontal.rowXs.push(0) }, _masonryHorizontalLayout: function (a) { var c = this, d = c.masonryHorizontal; a.each(function () { var a = b(this), e = Math.ceil(a.outerHeight(!0) / d.rowHeight); e = Math.min(e, d.rows); if (e === 1) c._masonryHorizontalPlaceBrick(a, d.rowXs); else { var f = d.rows + 1 - e, g = [], h, i; for (i = 0; i < f; i++) h = d.rowXs.slice(i, i + e), g[i] = Math.max.apply(Math, h); c._masonryHorizontalPlaceBrick(a, g) } }) }, _masonryHorizontalPlaceBrick: function (a, b) { var c = Math.min.apply(Math, b), d = 0; for (var e = 0, f = b.length; e < f; e++) if (b[e] === c) { d = e; break } var g = c, h = this.masonryHorizontal.rowHeight * d; this._pushPosition(a, g, h); var i = c + a.outerWidth(!0), j = this.masonryHorizontal.rows + 1 - f; for (e = 0; e < j; e++) this.masonryHorizontal.rowXs[d + e] = i }, _masonryHorizontalGetContainerSize: function () { var a = Math.max.apply(Math, this.masonryHorizontal.rowXs); return { width: a } }, _masonryHorizontalResizeChanged: function () { return this._checkIfSegmentsChanged(!0) }, _fitColumnsReset: function () { this.fitColumns = { x: 0, y: 0, width: 0 } }, _fitColumnsLayout: function (a) { var c = this, d = this.element.height(), e = this.fitColumns; a.each(function () { var a = b(this), f = a.outerWidth(!0), g = a.outerHeight(!0); e.y !== 0 && g + e.y > d && (e.x = e.width, e.y = 0), c._pushPosition(a, e.x, e.y), e.width = Math.max(e.x + f, e.width), e.y += g }) }, _fitColumnsGetContainerSize: function () { return { width: this.fitColumns.width } }, _fitColumnsResizeChanged: function () { return !0 }, _cellsByColumnReset: function () { this.cellsByColumn = { index: 0 }, this._getSegments(), this._getSegments(!0) }, _cellsByColumnLayout: function (a) { var c = this, d = this.cellsByColumn; a.each(function () { var a = b(this), e = Math.floor(d.index / d.rows), f = d.index % d.rows, g = (e + .5) * d.columnWidth - a.outerWidth(!0) / 2, h = (f + .5) * d.rowHeight - a.outerHeight(!0) / 2; c._pushPosition(a, g, h), d.index++ }) }, _cellsByColumnGetContainerSize: function () { return { width: Math.ceil(this.$filteredAtoms.length / this.cellsByColumn.rows) * this.cellsByColumn.columnWidth } }, _cellsByColumnResizeChanged: function () { return this._checkIfSegmentsChanged(!0) }, _straightAcrossReset: function () { this.straightAcross = { x: 0 } }, _straightAcrossLayout: function (a) { var c = this; a.each(function (a) { var d = b(this); c._pushPosition(d, c.straightAcross.x, 0), c.straightAcross.x += d.outerWidth(!0) }) }, _straightAcrossGetContainerSize: function () { return { width: this.straightAcross.x } }, _straightAcrossResizeChanged: function () { return !0 } }, b.fn.imagesLoaded = function (a) { function h() { a.call(c, d) } function i(a) { var c = a.target; c.src !== f && b.inArray(c, g) === -1 && (g.push(c), --e <= 0 && (setTimeout(h), d.unbind(".imagesLoaded", i))) } var c = this, d = c.find("img").add(c.filter("img")), e = d.length, f = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==", g = []; return e || h(), d.bind("load.imagesLoaded error.imagesLoaded", i).each(function () { var a = this.src; this.src = f, this.src = a }), c }; var w = function (b) { a.console && a.console.error(b) }; b.fn.isotope = function (a, c) { if (typeof a == "string") { var d = Array.prototype.slice.call(arguments, 1); this.each(function () { var c = b.data(this, "isotope"); if (!c) { w("cannot call methods on isotope prior to initialization; attempted to call method '" + a + "'"); return } if (!b.isFunction(c[a]) || a.charAt(0) === "_") { w("no such method '" + a + "' for isotope instance"); return } c[a].apply(c, d) }) } else this.each(function () { var d = b.data(this, "isotope"); d ? (d.option(a), d._init(c)) : b.data(this, "isotope", new b.Isotope(a, this, c)) }); return this } })(window, jQuery);