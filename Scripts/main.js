requirejs.config({
    //By default load any module IDs from js/lib
    baseUrl: 'Scripts',
    //except, if the module ID starts with "app",
    //load it from the js/app directory. paths
    //config is relative to the baseUrl, and
    //never includes a ".js" extension since
    //the paths config could be for a directory.
    paths: {
        jquery: ['http://apps.bdimg.com/libs/jquery/2.1.4/jquery.min.js', 'Scripts/jquery-2.1.1.min.js'],
        bootstrap: 'http://apps.bdimg.com/libs/bootstrap/3.3.0/js/bootstrap.min.js',
        modernizr: 'Scripts/modernizr.js',
        gridstack: '//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.2.4/gridstack.min.js',
        ladda: 'ladda',
        easyui: 'easyui-1.4.4',
        masonry: ['https://npmcdn.com/masonry-layout@4.0/dist/masonry.pkgd.min.js', 'masonry.pkgd.min'],
        jweixin: 'http://res.wx.qq.com/open/js/jweixin-1.0.0.js',
        lodash: 'Scripts/lodash.min.js',
        html5shiv: 'https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js',
        respond: 'https://oss.maxcdn.com/libs/respond.js/1.3.0/respond.min.js',
        cart: 'Scripts/jquery.cart.js',
        pager: 'Scripts/pager.js',
        jsrender: ['https://cdnjs.com/libraries/jsrender', 'Scripts/jsrender.min.js'],
        observable: '//www.jsviews.com/download/jquery.observable.min.js',
        jsviews: '//www.jsviews.com/download/jquery.views.min.js'
    },
    shim: {
        flexslider: ['jquery'],
        jtree: ['jtree']
    }
});

requirejs.onError = function (err) {
    console.log(err.requireType);
    if (err.requireType === 'timeout') {
        console.log('modules: ' + err.requireModules);
    }

    throw err;
};
