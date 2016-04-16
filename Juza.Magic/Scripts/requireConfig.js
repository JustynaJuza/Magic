require.config({
    baseUrl: "/Scripts/",
    paths: {
        jquery: "jquery-2.2.3",
        jqueryValidate: "jquery.validate",
        jqueryValidateUnobtrusive: "jquery.validate.unobtrusive",
        bootstrap: "bootstrap",
        moment: "moment"
    },
    shim: {
        jqueryValidate: ["jquery"],
        jqueryValidateUnobtrusive: ["jquery", "jqueryValidate"]
    }
});