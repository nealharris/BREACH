// http://javascript.info/tutorial/animation

function linear(progress) {
  return progress
}


function animateText(textArea, text) {
  if (text == null) {
    var text = textArea.value
  }

  var to = text.length, from = 0
  animate({
    delay: 20,
    duration: 5000,
    delta: bounceEaseOut,
//    delta: linear,
    step: function(delta) {
      var result = (to-from) * delta + from
      textArea.value = text.substr(0, Math.ceil(result))
    }
  })
}

function animateFinalText(textArea, text) {
  if (text == null) {
    var text = textArea.value
  }

  var to = text.length, from = 191
  animate({
    delay: 20,
    duration: 5000,
    delta: linear,
    step: function(delta) {
      var result = (to-from) * delta + from
      textArea.value = text.substr(0, Math.ceil(result))
    }
  })
}



function animate(opts) {
  var start = new Date
  var id = setInterval(function() {
    var timePassed = new Date - start
    var progress = timePassed / opts.duration
    if (progress > 1) progress = 1

    var delta = opts.delta(progress)
    opts.step(delta)

    if (progress == 1) {
      clearInterval(id)
    }
  }, opts.delay || 10)

}

function makeEaseInOut(delta) {
  return function(progress) {
    if (progress < .5)
      return delta(2*progress) / 2
    else
      return (2 - delta(2*(1-progress))) / 2
  }
}

function bounce(progress) {
  for(var a = 0, b = 1, result; 1; a += b, b /= 2) {
    if (progress >= (7 - 4 * a) / 11) {
      return -Math.pow((11 - 6 * a - 11 * progress) / 4, 2) + Math.pow(b, 2)
    }
  }
}

var bounceEaseOut = makeEaseInOut(bounce);

