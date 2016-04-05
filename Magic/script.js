//function PredatorApp(canvasId) {

// Returns a random integer between min (included) and max (included)
function getRandomIntInclusive(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

var $background = $('#background');
var $backgroundWidth = $('#background-width');
var $backgroundHeight = $('#background-height');

function randomizeSize() {
    var width = getRandomIntInclusive(100, 400);
    var height = getRandomIntInclusive(100, 450);
    $backgroundWidth.val(width);
    $background.css({ width: width });
    $backgroundHeight.val(height);
    $background.css({ height: height });
}

$backgroundWidth.change(() => $background.css({ width: $backgroundWidth.val() }));
$backgroundHeight.change(() => $background.css({ height: $backgroundHeight.val() }));

randomizeSize();

window.singleFieldWidth = 10;

var $singleFieldWidth = $('#single-field-width');
$singleFieldWidth.change(updateFieldSize);

window.rows = [4, 5, 12, 3, 8, 9, 14];

window.fieldCoords = [
    [0, 0],
    [0, window.singleFieldWidth],
    [window.singleFieldWidth, window.singleFieldWidth],
    [window.singleFieldWidth, 0]];

var svg = d3.select('#some-container')
    .append('svg:svg')
    .attr('height', $backgroundHeight.val())
    .attr('width', $backgroundWidth.val());

var drawLine = d3.svg.line()
                .x(function (d) { return d[0]; })
                .y(function (d) { return d[1]; })
                .interpolate('linear');

function render() {
    svg.data(window.rows).enter()
        .append('svg:path')
        .attr('d', drawLine(window.fieldCoords) + 'Z')
        .style('stroke-width', 1)
        .style('stroke', '#FFFF00');
}

function updateFieldSize() {
    window.singleFieldWidth = $singleFieldWidth.val();

    window.fieldCoords = [
        [0, 0],
        [0, window.singleFieldWidth],
        [window.singleFieldWidth, window.singleFieldWidth],
        [window.singleFieldWidth, 0]];

    svg.selectAll('*').remove();
    render();
}

render();


//function Shape(x, y, w, h, fill) {
//    // This is a very simple and unsafe constructor. 
//    // All we're doing is checking if the values exist.
//    // "x || 0" just means "if there is a value for x, use that. Otherwise use 0."
//    this.x = x || 0;
//    this.y = y || 0;
//    this.w = w || 10;
//    this.h = h || 10;
//    this.fill = fill || '#AAAAAA';
//}

//// Draws this shape to a given context
//Shape.prototype.draw = function(ctx) {
//    ctx.fillStyle = this.fill;
//    ctx.fillRect(this.x, this.y, this.w, this.h);
//}

//    var HEIGHT = getRandomIntInclusive(100, 300);
//    var WIDTH = getRandomIntInclusive(100, 300);

//var CANVAS = document.getElementById("canvas");
//var ctx = canvas.getContext("2d");


//function generateGrid() {
//    for(var i = 0; i < WIDTH){
//        ctx.fillRect(0,0,150,75);
//    }
//}

//function drawGrid() {
//    for (var x = 0; x <= WIDTH; x += 40) {
//        context.moveTo(0.5 + x + p, p);
//        context.lineTo(0.5 + x + p, bh + p);
//    }


//    for (var x = 0; x <= HEIGHT; x += 40) {
//        context.moveTo(p, 0.5 + x + p);
//        context.lineTo(bw + p, 0.5 + x + p);
//    }

//    context.strokeStyle = "black";
//    context.stroke();
//}
//}

//PredatorApp();