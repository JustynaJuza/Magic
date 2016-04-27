require.config({
    baseUrl: '/Scripts/',
    paths: {
        jquery: 'jquery-2.2.3',
        jqueryValidate: 'jquery.validate',
        jqueryValidateUnobtrusive: 'jquery.validate.unobtrusive',
        bootstrap: 'bootstrap',
        moment: 'moment',
        lodash: 'lodash',
        signalr: 'jquery.signalR-2.2.0',
        'signalr-hubs': '/signalr/hubs?',
        'hub-start': 'global/hub-start',
        chat: 'global/chat'
    },
    shim: {
        jqueryValidate: ['jquery'],
        jqueryValidateUnobtrusive: ['jquery', 'jqueryValidate'],
        signalr: ['jquery'],
        'signalr-hubs': ['signalr'],
        chat: ['signalr-hubs', 'lodash'],
        'hub-start': ['chat']
    }
});