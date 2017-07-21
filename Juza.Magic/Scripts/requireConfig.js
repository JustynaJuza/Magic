require.config({
    // for development, prevent caching:
    urlArgs: "refresh=" + (new Date()).getTime(),
    
    baseUrl: '/Scripts/',
    paths: {
        jquery: 'jquery-3.1.0',
        jqueryValidate: 'jquery.validate',
        jqueryValidateUnobtrusive: 'jquery.validate.unobtrusive',
        bootstrap: 'bootstrap',
        moment: 'moment',
        lodash: 'lodash',
        signalr: 'jquery.signalR-2.2.1',
        signalrHubs: '/signalr/hubs?',
        react: 'react',
        reactDom: 'react-dom',

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
        helpers: ['jquery'],
        signalr: ['jquery'],
        signalrHubs: ['signalr'],
        reactDom: ['react'],
        chatVariables: ['jquery'],
        chatHub: ['chatVariables', 'signalrHubs'],
        chatInteraction: ['chatVariables', 'helpers'],
        chatDisplay: ['chatVariables'],
        chatFunctionality: ['chatVariables'],
        chat: ['signalrHubs', 'lodash', 'chatVariables', 'chatFunctionality', 'chatHub', 'chatDisplay', 'chatInteraction'],
        hubStart: ['signalrHubs']
    }
});