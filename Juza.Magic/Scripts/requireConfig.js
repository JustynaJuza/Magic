require.config({
    // for development, prevent caching:
    urlArgs: "refresh=" + (new Date()).getTime(),
    
    baseUrl: '/Scripts/',
    paths: {
        jquery: 'jquery-2.2.3',
        jqueryValidate: 'jquery.validate',
        jqueryValidateUnobtrusive: 'jquery.validate.unobtrusive',
        bootstrap: 'bootstrap',
        moment: 'moment',
        lodash: 'lodash',
        signalr: 'jquery.signalR-2.2.0',
        signalrHubs: '/signalr/hubs?',

        helpers: 'utilities/helpers',

        chat: 'global/chat/chat',
        chatVariables: 'global/chat/chat.variables',
        chatFunctionality: 'global/chat/chat.functionality',
        chatHub: 'global/chat/chat.hub',
        chatDisplay: 'global/chat/chat.display',
        chatInteraction: 'global/chat/chat.interaction',

        hubStart: 'global/hub-start'
    },
    shim: {
        jqueryValidate: ['jquery'],
        jqueryValidateUnobtrusive: ['jquery', 'jqueryValidate'],
        signalr: ['jquery'],
        signalrHubs: ['signalr'],
        chatVariables: ['jquery'],
        chatHub: ['chatVariables', 'signalrHubs'],
        chatInteraction: ['chatVariables', 'helpers'],
        chatDisplay: ['chatVariables'],
        chatFunctionality: ['chatVariables'],
        chat: ['signalrHubs', 'lodash', 'chatVariables', 'chatFunctionality', 'chatHub', 'chatDisplay', 'chatInteraction'],
        hubStart: ['signalrHubs']
    }
});