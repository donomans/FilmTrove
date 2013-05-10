jQuery(document).ready(function() {


/* ---------------- Main Navigation ----------------- */
	
	
	/* Search Form*/
	
	$("#searchbox").on('keyup',function(){
		
		$(this).attr('class', 'search-form active-search');
		
		var count;
		var timeToEnd = 1000;

		$("#searchbox").keydown(function () {
		    if (event.keyCode == 13) {
		        document.forms['searchform'].submit();
		    }

		    clearTimeout(count);
			count = setTimeout(endCount, timeToEnd);
		});
		
	});
	function endCount() {
	    $("#searchbox").attr('class', 'search-form');
	}
	var cache = {};
	$("#searchbox").autocomplete({
	    minLenght: 2,
	    delay: 250,
	    source: function (request, response) {
	        var term = $.trim(request.term);
	        if (term in cache){
	            response(cache[term]);
	            return;
	        }
                
	        $.getJSON("/api/v1/search/netflix/autocomplete",
                request,
                function (data, status, xhr) {
                    cache[term] = data;
                    response(data);
                });
	    }
	});

	
	/* Menu */
	(function() {

		var $mainNav    = $('#navigation').children('ul');

		$mainNav.on('mouseenter', 'li', function() {
			var $this    = $(this),
				$subMenu = $this.children('ul');
			if( $subMenu.length ) $this.addClass('hover');
			$subMenu.hide().stop(true, true).slideDown('fast');
		}).on('mouseleave', 'li', function() {
			$(this).removeClass('hover').children('ul').stop(true, true).slideUp('fast');
		});
		
	})();

    /* Menu */
	(function () {

	    var $addtoNav = $('.addtonavigation').children('ul');

	    $addtoNav.on('mouseenter', 'li', function () {
	        var $this = $(this),
				$subMenu = $this.children('ul');
	        if ($subMenu.length) $this.addClass('hover');
	        $subMenu.hide().stop(true, true).slideDown('fast');
	    }).on('mouseleave', 'li', function () {
	        $(this).removeClass('hover').children('ul').stop(true, true).slideUp('fast');
	    });

	})();
	
	/* Responsive Menu */
	(function() {
		selectnav('nav', {
			label: 'Menu',
			nested: true,
			indent: '-'
		});
				
	})();

/* ------------------ Image Overlay ----------------- */

	//$(document).ready(function () {
	//  $('.picture a').hover(function () {
	//	$(this).find('.image-overlay-zoom, .image-overlay-link').stop().fadeTo('fast', 1);
	//  },function () {
	//	$(this).find('.image-overlay-zoom, .image-overlay-link').stop().fadeTo('fast', 0);
	//  });
	//});


/* ------------------ Back To Top ------------------- */
	
	//jQuery('#scroll-top-top a').click(function(){
	//	jQuery('html, body').animate({scrollTop:0}, 300); 
	//	return false; 
	//}); 
	
/* ------------------- Accordion -------------------- */	

	(function() {
		var $container = $('.acc-container'),
			$trigger   = $('.acc-trigger');

		$container.hide();
		$trigger.first().addClass('active').next().show();

		var fullWidth = $container.outerWidth(true);
		$trigger.css('width', fullWidth);
		$container.css('width', fullWidth);
		
		$trigger.on('click', function(e) {
			if( $(this).next().is(':hidden') ) {
				$trigger.removeClass('active').next().slideUp(300);
				$(this).toggleClass('active').next().slideDown(300);
			}
			e.preventDefault();
		});

		$(window).on('resize', function() {
			fullWidth = $container.outerWidth(true)
			$trigger.css('width', $trigger.parent().width() );
			$container.css('width', $container.parent().width() );
		});

	})();
/* ------------------ Alert Boxes ------------------- */	

jQuery(document).ready(function()
{
	jQuery(document.body).pixusNotifications({
			speed: 300,
			animation: 'fadeAndSlide',
			hideBoxes: false
	});
});

(function()
{
	$.fn.pixusNotifications = function(options)
	{
		var defaults = {
			speed: 200,
			animation: 'fade',
			hideBoxes: false
		};
		
		var options = $.extend({}, defaults, options);
		
		return this.each(function()
		{
			var wrapper = $(this),
				notification = wrapper.find('.notification'),
				content = notification.find('p'),
				title = content.find('strong'),
				closeBtn = $('<a class="close" href="#"></a>');
			
			$(document.body).find('.notification').each(function(i)
			{
				var i = i+1;
				$(this).attr('id', 'notification_'+i);
			});
			
			notification.filter('.closeable').append(closeBtn);
			
			closeButton = notification.find('> .close');
			
			closeButton.click(function()
			{
				hideIt( $(this).parent() );
				return false;
			});			
			
			function hideIt(object)
			{
				switch(options.animation)
				{
					case 'fade': fadeIt(object);     break;
					case 'slide': slideIt(object);     break;
					case 'box': boxAnimIt(object);     break;
					case 'fadeAndSlide': fadeItSlideIt(object);     break;
					default: fadeItSlideIt(object);
				}
			};
			
			function fadeIt(object)
			{	object
				.fadeOut(options.speed);
			}			
			function slideIt(object)
			{	object
				.slideUp(options.speed);
			}			
			function fadeItSlideIt(object)
			{	object
				.fadeTo(options.speed, 0, function() { slideIt(object) } );
			}			
			function boxAnimIt(object)
			{	object
				.hide(options.speed);
			}
			
			if (options.hideBoxes){}
			
			else if (! options.hideBoxes)
			{
				notification.css({'display': 'block', 'visiblity': 'visible'});
			}
			
		});
	};
})();

/*----------------------------------------------------*/
/*	Tabs
/*----------------------------------------------------*/

	(function() {

		var $tabsNav    = $('.tabs-nav'),
			$tabsNavLis = $tabsNav.children('li'),
			$tabContent = $('.tab-content');

		$tabsNav.each(function() {
			var $this = $(this);

			$this.next().children('.tab-content').stop(true,true).hide()
												 .first().show();

			$this.children('li').first().addClass('active').stop(true,true).show();
		});

		$tabsNavLis.on('click', function(e) {
			var $this = $(this);

			$this.siblings().removeClass('active').end()
				 .addClass('active');
			
			$this.parent().next().children('.tab-content').stop(true,true).hide()
														  .siblings( $this.find('a').attr('href') ).fadeIn();

			e.preventDefault();
		});

	})();

/* -------------------- Isotope --------------------- */

$('#wrapper').imagesLoaded(function() {
		var $container = $('#portfolio-wrapper');
				$select = $('#filters select');
				
		// initialize Isotope
		$container.isotope({
		  // options...
		  resizable: false, // disable normal resizing
		  // set columnWidth to a percentage of container width
		  masonry: { columnWidth: $container.width() / 12 }
		});

		// update columnWidth on window resize
		$(window).smartresize(function(){
		  $container.isotope({
			// update columnWidth to a percentage of container width
			masonry: { columnWidth: $container.width() / 12 }
		  });
		});
		
		
	  $container.isotope({
		itemSelector : '.portfolio-item'
	  });
	  
	$select.change(function() {
			var filters = $(this).val();
	
			$container.isotope({
				filter: filters
			});
		});
	  
	  var $optionSets = $('#filters .option-set'),
		  $optionLinks = $optionSets.find('a');

	  $optionLinks.click(function(){
		var $this = $(this);
		// don't proceed if already selected
		if ( $this.hasClass('selected') ) {
		  return false;
		}
		var $optionSet = $this.parents('.option-set');
		$optionSet.find('.selected').removeClass('selected');
		$this.addClass('selected');
  
		// make option object dynamically, i.e. { filter: '.my-filter-class' }
		var options = {},
			key = $optionSet.attr('data-option-key'),
			value = $this.attr('data-option-value');
		// parse 'false' as false boolean
		value = value === 'false' ? false : value;
		options[ key ] = value;
		if ( key === 'layoutMode' && typeof changeLayoutMode === 'function' ) {
		  // changes in layout modes need extra logic
		  changeLayoutMode( $this, options )
		} else {
		  // otherwise, apply new options
		  $container.isotope( options );
		}
		
		return false;
	  });
});
	
/* ------------------- Fancybox --------------------- */

(function() {

	$('[rel=image]').fancybox({
		type        : 'image',
		openEffect  : 'fade',
		closeEffect	: 'fade',
		nextEffect  : 'fade',
		prevEffect  : 'fade',
		helpers     : {
			title   : {
				type : 'inside'
			}
		}
	});
	
	$('[rel=image-gallery]').fancybox({
		nextEffect  : 'fade',
		prevEffect  : 'fade',
		helpers     : {
			title   : {
				type : 'inside'
			},
			buttons  : {},
			media    : {}
		}
	});
	
	$(".addtocollection").fancybox({
	    type: 'ajax',
	    ajax: {
	        dataType: 'html',
	        headers:
            { 'Accept': 'application/json' }
	    },
	    maxWidth: 400,
	    maxHeight: 800,
	    fitToView: true,
	    width: '50%',
	    height: '50%',
	    autoSize: true,
	    closeClick: false,
	    openEffect: 'none',
	    closeEffect: 'none'
	});
	
})();

});


jQuery(document).ready(function($){

	// Add Active Class To Current Link
	var get_url = window.location.pathname.split( '/' ); // get current URL
	var url = get_url.slice(-1)[0]
		
	if (url == 0) {
		
		$('#nav a[href="/"]').addClass('active');
		
	} else {
		
		$('#nav a[href="/'+url+'"]').addClass('active');
		
	}
	
	var $activeUL = $('.active').closest('ul');
	/* Revise below condition that tests if .active is a submenu */
	if($activeUL.attr('id') != 'nav') { //check if it is submenu
	    $activeUL
	        .parent() //This should return the li
	        .children('a') //The childrens are <a> and <ul>
	        .addClass('active'); //add class active to the a    
	}
});