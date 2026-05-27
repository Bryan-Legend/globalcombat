/// <reference path="Scripts/jquery-1.7.2.js" />
/// <reference path="Scripts/jquery.signalR-0.5.1.js" />
/// <reference path="/signalr/hubs" />

// source: http://sam-benne.co.uk/code/jquery-html5-notification/
;(function ($) {
    $.desknoty = function (options) {
        var defaults = {
            icon: null,
            title: "",
            body: "",
            timeout: 5000,
            sticky: false,
            id: null,
            type: 'normal',
            url: '',
            dir: '',
            onClick: function () { },
            onShow: function () { },
            onClose: function () { },
            onError: function () { }
        }

        var p = this, noti = null;

        p.set = {}

        var init = function () {
            p.set = $.extend({}, defaults, options);
            if (isSupported()) {
                if (window.webkitNotifications.checkPermission() != 0) {
                    getPermissions(init);
                } else {
                    if (p.set.type === 'normal') createNoti();
                    else if (p.set.type === 'html') createNotiHtml();
                }
            }/* else {
                alert("Desktop notifications are not supported!");
            }*/
        }

        var createNoti = function () {
            if (!isSupported())
                return;
            noti = window.webkitNotifications.createNotification(p.set.icon, p.set.title, p.set.body);
            if (p.set.dir) noti.dir = p.set.dir;
            if (p.set.onclick) noti.onclick = p.set.onclick;
            if (p.set.onshow) noti.onshow = p.set.onshow;
            if (p.set.onclose) noti.onclose = p.set.onclose;
            if (p.set.onerror) noti.onerror = p.set.onerror;
            if (p.set.id) noti.replaceId = p.set.id;
            noti.show();
            if (!p.set.sticky) setTimeout(function () { noti.cancel(); }, p.set.timeout);
        }
        var createNotiHtml = function () {
            if (!isSupported())
                return;
            noti = window.webkitNotifications.createHTMLNotification(p.set.url);
            if (p.set.dir) noti.dir = p.set.dir;
            if (p.set.onclick) noti.onclick = p.set.onclick;
            if (p.set.onshow) noti.onshow = p.set.onshow;
            if (p.set.onclose) noti.onclose = p.set.onclose;
            if (p.set.onerror) noti.onerror = p.set.onerror;
            if (p.set.id) noti.replaceId = p.set.id;
            noti.show();
            if (!p.set.sticky) setTimeout(function () { noti.cancel(); }, p.set.timeout);
        }

        var isSupported = function () {
            if (window.webkitNotifications) return true;
            else return false;
        }
        var getPermissions = function (callback) {
            window.webkitNotifications.requestPermission(callback);
        }
        init();
    }
})(jQuery);











/**
* noty - jQuery Notification Plugin v1.2.1
* Contributors: https://github.com/needim/noty/graphs/contributors
*
* Examples and Documentation - http://needim.github.com/noty/
*
* Licensed under the MIT licenses:
* http://www.opensource.org/licenses/mit-license.php
*
**/
(function ($) {
    $.noty = function (options, customContainer) {

        var base = {};
        var $noty = null;
        var isCustom = false;

        base.init = function (options) {
            base.options = $.extend({}, $.noty.defaultOptions, options);
            base.options.type = base.options.cssPrefix + base.options.type;
            base.options.id = base.options.type + '_' + new Date().getTime();
            base.options.layout = base.options.cssPrefix + 'layout_' + base.options.layout;

            if (base.options.custom.container) customContainer = base.options.custom.container;
            isCustom = ($.type(customContainer) === 'object') ? true : false;

            return base.addQueue();
        };

        // Push notification to queue
        base.addQueue = function () {
            var isGrowl = ($.inArray(base.options.layout, $.noty.growls) == -1) ? false : true;
            if (!isGrowl) (base.options.force) ? $.noty.queue.unshift({ options: base.options }) : $.noty.queue.push({ options: base.options });
            return base.render(isGrowl);
        };

        // Render the noty
        base.render = function (isGrowl) {

            // Layout spesific container settings
            var container = (isCustom) ? customContainer.addClass(base.options.theme + ' ' + base.options.layout + ' noty_custom_container') : $('body');
            if (isGrowl) {
                if ($('ul.noty_cont.' + base.options.layout).length == 0)
                    container.prepend($('<ul/>').addClass('noty_cont ' + base.options.layout));
                container = $('ul.noty_cont.' + base.options.layout);
            } else {
                if ($.noty.available) {
                    var fromQueue = $.noty.queue.shift(); // Get noty from queue
                    if ($.type(fromQueue) === 'object') {
                        $.noty.available = false;
                        base.options = fromQueue.options;
                    } else {
                        $.noty.available = true; // Queue is over
                        return base.options.id;
                    }
                } else {
                    return base.options.id;
                }
            }
            base.container = container;

            // Generating noty bar
            base.bar = $('<div class="noty_bar"/>').attr('id', base.options.id).addClass(base.options.theme + ' ' + base.options.layout + ' ' + base.options.type);
            $noty = base.bar;
            $noty.append(base.options.template).find('.noty_text').html(base.options.text);
            $noty.data('noty_options', base.options);

            // Close button display
            (base.options.closeButton) ? $noty.addClass('noty_closable').find('.noty_close').show() : $noty.find('.noty_close').remove();

            // Bind close event to button
            $noty.find('.noty_close').bind('click', function () { $noty.trigger('noty.close'); });

            // If we have a button we must disable closeOnSelfClick and closeOnSelfOver option
            if (base.options.buttons) base.options.closeOnSelfClick = base.options.closeOnSelfOver = false;
            // Close on self click
            if (base.options.closeOnSelfClick) $noty.bind('click', function () { $noty.trigger('noty.close'); }).css('cursor', 'pointer');
            // Close on self mouseover
            if (base.options.closeOnSelfOver) $noty.bind('mouseover', function () { $noty.trigger('noty.close'); }).css('cursor', 'pointer');

            // Set buttons if available
            if (base.options.buttons) {
                $buttons = $('<div/>').addClass('noty_buttons');
                $noty.find('.noty_message').append($buttons);
                $.each(base.options.buttons, function (i, button) {
                    bclass = (button.type) ? button.type : 'gray';
                    $button = $('<button/>').addClass(bclass).html(button.text).appendTo($noty.find('.noty_buttons'))
					.bind('click', function () {
					    if ($.isFunction(button.click)) {
					        button.click.call($button, $noty);
					    }
					});
                });
            }

            return base.show(isGrowl);
        };

        base.show = function (isGrowl) {

            // is Modal?
            if (base.options.modal) $('<div/>').addClass('noty_modal').addClass(base.options.theme).prependTo($('body')).fadeIn('fast');

            $noty.close = function () { return this.trigger('noty.close'); };

            // Prepend noty to container
            (isGrowl) ? base.container.prepend($('<li/>').append($noty)) : base.container.prepend($noty);

            // topCenter and center specific options
            if (base.options.layout == 'noty_layout_topCenter' || base.options.layout == 'noty_layout_center') {
                $.noty.reCenter($noty);
            }

            $noty.bind('noty.setText', function (event, text) {
                $noty.find('.noty_text').html(text);

                if (base.options.layout == 'noty_layout_topCenter' || base.options.layout == 'noty_layout_center') {
                    $.noty.reCenter($noty);
                }
            });

            $noty.bind('noty.setType', function (event, type) {
                $noty.removeClass($noty.data('noty_options').type);

                type = $noty.data('noty_options').cssPrefix + type;

                $noty.data('noty_options').type = type;

                $noty.addClass(type);

                if (base.options.layout == 'noty_layout_topCenter' || base.options.layout == 'noty_layout_center') {
                    $.noty.reCenter($noty);
                }
            });

            $noty.bind('noty.getId', function (event) {
                return $noty.data('noty_options').id;
            });

            // Bind close event
            $noty.one('noty.close', function (event) {
                var options = $noty.data('noty_options');
                if (options.onClose) { options.onClose(); }

                // Modal Cleaning
                if (options.modal) $('.noty_modal').fadeOut('fast', function () { $(this).remove(); });

                $noty.clearQueue().stop().animate(
						$noty.data('noty_options').animateClose,
						$noty.data('noty_options').speed,
						$noty.data('noty_options').easing,
						$noty.data('noty_options').onClosed)
				.promise().done(function () {

				    // Layout spesific cleaning
				    if ($.inArray($noty.data('noty_options').layout, $.noty.growls) > -1) {
				        $noty.parent().remove();
				    } else {
				        $noty.remove();

				        // queue render
				        $.noty.available = true;
				        base.render(false);
				    }

				});
            });

            // Start the show
            if (base.options.onShow) { base.options.onShow(); }
            $noty.animate(base.options.animateOpen, base.options.speed, base.options.easing, base.options.onShown);

            // If noty is have a timeout option
            if (base.options.timeout) $noty.delay(base.options.timeout).promise().done(function () { $noty.trigger('noty.close'); });
            return base.options.id;
        };

        // Run initializer
        return base.init(options);
    };

    // API
    $.noty.get = function (id) { return $('#' + id); };
    $.noty.close = function (id) {
        //remove from queue if not already visible
        for (var i = 0; i < $.noty.queue.length; ) {
            if ($.noty.queue[i].options.id == id)
                $.noty.queue.splice(id, 1);
            else
                i++;
        }
        //close if already visible
        $.noty.get(id).trigger('noty.close');
    };
    $.noty.setText = function (id, text) {
        $.noty.get(id).trigger('noty.setText', text);
    };
    $.noty.setType = function (id, type) {
        $.noty.get(id).trigger('noty.setType', type);
    };
    $.noty.closeAll = function () {
        $.noty.clearQueue();
        $('.noty_bar').trigger('noty.close');
    };
    $.noty.reCenter = function (noty) {
        noty.css({ 'left': ($(window).width() - noty.outerWidth()) / 2 + 'px' });
    };
    $.noty.clearQueue = function () {
        $.noty.queue = [];
    };

    var windowAlert = window.alert;
    $.noty.consumeAlert = function (options) {
        window.alert = function (text) {
            if (options) { options.text = text; }
            else { options = { text: text }; }
            $.noty(options);
        };
    }
    $.noty.stopConsumeAlert = function () {
        window.alert = windowAlert;
    }

    $.noty.queue = [];
    $.noty.growls = ['noty_layout_topLeft', 'noty_layout_topRight', 'noty_layout_bottomLeft', 'noty_layout_bottomRight'];
    $.noty.available = true;
    $.noty.defaultOptions = {
        layout: 'top',
        theme: 'noty_theme_default',
        animateOpen: { height: 'toggle' },
        animateClose: { height: 'toggle' },
        easing: 'swing',
        text: '',
        type: 'alert',
        speed: 500,
        timeout: 5000,
        closeButton: false,
        closeOnSelfClick: true,
        closeOnSelfOver: false,
        force: false,
        onShow: false,
        onShown: false,
        onClose: false,
        onClosed: false,
        buttons: false,
        modal: false,
        template: '<div class="noty_message"><span class="noty_text"></span><div class="noty_close"></div></div>',
        cssPrefix: 'noty_',
        custom: {
            container: null
        }
    };

    $.fn.noty = function (options) {
        return this.each(function () {
            (new $.noty(options, $(this)));
        });
    };

})(jQuery);

//Helper
function noty(options) {
    return jQuery.noty(options); // returns an id
}












function PlayNotifySound() {
    document.getElementById('notify').play();
}

function AppendChatMessage(sourceId, sourceName, message) {
    if ($('div[title="' + sourceId + '"]').length == 0) {
        $.popupChat(sourceId, sourceName);
        PlayNotifySound();
    } else {
        var chatArea = $('div[title="' + sourceId + '"] .chat_area');
        chatArea.append(" <b>" + sourceName + ":</b> " + message + "<br />");
        chatArea.animate({ scrollTop: 9999999 }, 1);

        //                if (this_chatbox.data('havenewmessage') == 1 && this_chatbox.data('fistload') == 1) { // blinking chat window if not focused and have a new message
        //                    if (this_chatbox.data('blink') == 0) {
        //                        this_chatbox_headerbg.removeClass("header_bg_default").addClass("header_bg_blink");
        //                        this_chatbox.data('blink', 1);
        //                    } else {
        //                        this_chatbox.data('blink', 0);
        //                        this_chatbox_headerbg.removeClass("header_bg_blink").addClass("header_bg_default");
        //                    }
        //                }
    }
}

(function ($) {
    history.navigationMode = 'compatible'; //for Opera to support unload function

    $.popupChat = function (partner_id, partner_username, offline) {
        $.popupChat.windowscount++;
        var wctr = $.popupChat.windowscount;

        $('body').append('<div class="chatbox cb_default" id="chat_window_' + wctr + '" title="' + partner_id + '">' +
				'<div class="header header_bg_default" title="' + partner_username + '">' +
					partner_username + (offline ? " - Offline" : "") +
					'<a href="#" class="close_chatbox" title="close chat window">X</a>' +
					'<a href="#" class="minimize_chatbox" title="minimize chat window">_</a>' +
					'<a href="#" class="maximize_chatbox" title="maximize chat window">&#8254;</a>' +
				'</div>' +
				'<div class="chat_area" title="' + partner_username + '">' +
				'</div>' +
//				'<div class="chat_info"><p></p></div>' +
				'<div class="chat_message" title="Type your message here">' +
					'<textarea></textarea>' +
				'</div>' +
			'</div>');

        //check if already exist any window on page and set nu position
        if (wctr > 0) {
            //get last chat window position
            var prev_w = wctr - 1;
            var prev_w_pos = parseInt($('#chat_window_' + prev_w).css('right'));
            var nu_w_pos = prev_w_pos + 225;
            $('#chat_window_' + wctr).css('right', nu_w_pos + 'px');
        }

        $.post("/Home/LoadChatMessages", { targetId: partner_id, targetName: partner_username },
            function (result)
            {
                $.each(result, function (index, message) {
                    AppendChatMessage(partner_id, message.name, message.text);
                });
            }
        );

        return false;
    }

    $.popupChat.windowscount = 0;

    $(document).ready(function () {
        $('.minimize_chatbox').live('click', function () {
            //remove chat,message area			
            $(this).closest('.chatbox').find('.chat_area,.chat_message,.chat_info').css('height', '0px');
            $(this).closest('.chatbox').css('height', '25px');

            //replace minimize icon
            $(this).css('display', 'none');
            $(this).closest('.chatbox').find('.maximize_chatbox').css('display', 'inline');

            return false;
        });

        $('.maximize_chatbox').live('click', function () {
            //remove chat,message area			
            $(this).closest('.chatbox').find('.chat_area').css('height', '180px');
            $(this).closest('.chatbox').find('.chat_message').css('height', '55px');
            $(this).closest('.chatbox').find('.chat_info').css('height', '20px');
            $(this).closest('.chatbox').css('height', '300px');
            $(this).closest('.chatbox').find('.header_bg_blink').removeClass("header_bg_blink").addClass("header_bg_default");

            //replace minimize icon
            $(this).css('display', 'none');
            $(this).closest('.chatbox').find('.minimize_chatbox').css('display', 'inline');
            $(this).closest('.chatbox').find('.header .new_message').remove();
            $(this).closest('.chatbox').find('.chat_message textarea').focus();
            return false;
        });

        $('.close_chatbox').live('click', function () {
            var targetChatbox = $(this).closest('.chatbox');
            var to_id = targetChatbox.attr("title");
            var to_name = targetChatbox.children().attr("title");
			var closed_pos = parseInt(targetChatbox.css('right'));
            targetChatbox.remove();

            //set nu position for all appearing chat window
            $('.chatbox').each(function () {
                var prev_pos = parseInt($(this).css('right'));
				if (prev_pos != 10 && (prev_pos > closed_pos )){
                    var nu_pos = prev_pos - 225;
                    $(this).css('right', nu_pos + 'px');
                }
            });

            $.post("/Home/CloseChatWindow", { targetId: to_id, targetName: to_name });

            return false;
        });

        //ON USER CLICK POP UP CHAT
        $('.chat_user').live('click', function () {
            var offline = false;
            var substr = $(this).attr('alt').split('|');
            var user_id = substr[0];
            var user_name = substr[1];
            if ($(this).attr('offline') == 1)
                offline = true;

            //check if a windows is already open with this user first!
            if ($('div[title="' + user_id + '"]').length > 0) {
                //alert('You\'re already chatting with him/her!');
            } else {
                $.popupChat(user_id, user_name, offline);
            }

            $('div[title="' + user_id + '"] textarea').focus();
        });

        // set focus in Message area
        $('.chatbox').live('click', function () {
            $textarea = $('.chat_message textarea', this);
            $textarea.focus();
        });

        //PRINT A LINE TO CHAT 
        function print_to_chat(window_id, text) {
            $('#' + window_id + ' .chat_area').append(text);
        }

        //HIGHLIGHT Active chat window
        $('.chat_message textarea').live('focus', function () {
            var chatbox = $(this).closest('.chatbox');
            this_chatbox_headerbg = $('.header', chatbox);
            this_chatbox_headerbg.removeClass("header_bg_blink").addClass("header_bg_default");
            chatbox.removeClass("cb_default").addClass("cb_highlight"); // add highligt to chat window
            chatbox.data('focused', 1); 	   // enable focus variable
            chatbox.data('havenewmessage', 0); // clear new message
        });
        $('.chat_message textarea').live('blur', function () {
            var chatbox = $(this).closest('.chatbox');
            chatbox.removeClass("cb_highlight").addClass("cb_default"); // remove highligt of chat window
            chatbox.data('focused', 0); // disable focus variable
        });

        //SEND MESSAGE ON ENTER		
        $('.chat_message textarea').live('keypress', function (e) {
            if (e.keyCode == 13 && !e.shiftKey) {
                e.preventDefault();

                var to_id = $(this).closest('.chatbox').attr('title');
                var this_chat_window_id = $(this).closest('.chatbox').attr('id');
                var this_textarea = $(this);

                $.post("/Home/Chat", { targetId: to_id, message: this_textarea.val() },
                    function (result)
                    {
                        AppendChatMessage(to_id, "Me", this_textarea.val());
                        this_textarea.val('');
                    }
                );
            } else {
                //user is typing...
                //$is_typing = 1;	//1.2 - Remove this line
            }
        });

        $('.chat_message textarea').live('keyup', function () {
            //$is_typing = 0; //1.2 - Remove this line
        });

        //USER GOES OFFLINE ON WINDOW CLOSE/NAVIGATE AWAY
//        $(window).unload(function () {
//            $.ajax({
//                type: "POST",
//                url: 'set_status.php',
//                async: false,
//                data: 'own_id=' + $own_id + '&status=offline',
//                success: function (i) {
//                    //do nothing												
//                }
//            });
//        });

//                //check if user is typing
//                $.ajax({
//                    type: "POST",
//                    url: 'is_typing.php',
//                    data: 'own_id=' + $own_id + '&partner_id=' + $(this).attr('title'),
//                    success: function (e) {
//                        if (e == 1) {
//                            $('.chatbox[title="' + this_chatbox_id + '"] .chat_info p').text('User is typing...');
//                        } else {
//                            $('.chatbox[title="' + this_chatbox_id + '"] .chat_info p').text('');
//                        }
//                    }
//                });
//            });
    });
})(jQuery);




$(function () {
    var isFocused;
    $(window).blur(function () {
        isFocused = false;
    });
    $(window).focus(function () {
        isFocused = true;
    });

    var gameHub = $.connection.gameHub;

    gameHub.sendNotification = function (title, message, targetUri) {
        if (window.location.pathname != targetUri || !isFocused) {
            PlayNotifySound();

            $.desknoty({
                icon: "/images/icon.png",
                title: title,
                body: message
            });

            noty({ "text": title + "<br /> " + message, "layout": "bottomRight", "closeButton": true });
        }
    };

    gameHub.reload = function () {
        location.reload();
    };

    gameHub.recieveMessage = function (sourceId, sourceName, message) {
        AppendChatMessage(sourceId, sourceName, message);
    };

    $.connection.hub.start();
});