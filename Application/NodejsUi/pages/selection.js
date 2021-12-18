const response = fetch('http://localhost:5000/api/images/labels')
    .then((response) => {
        return response.json();
    })
    .then((data) => {
        console.log(data);
        var selectionEl = document.getElementById('labels');
        for (var i = 0; i < data.length; i++) {
            var opt = document.createElement('option');
            opt.innerHTML = data[i];
            selectionEl.appendChild(opt);
        }
    });

var counter = document.getElementById('image_counter');
counter.innerText = 'Number of images: 0';

var slideIndex = 1;
showSlides(slideIndex);

function plusSlides(n) {
    showSlides(slideIndex += n);
}

function currentSlide(n) {
    showSlides(slideIndex = n);
}

function showSlides(n) {
    var i;
    var slides = document.getElementsByClassName("mySlides");
    if (n > slides.length) {
        slideIndex = 1
    }
    if (n < 1) {
        slideIndex = slides.length
    }
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    slides[slideIndex - 1].style.display = "block";
}


function changeFunc() {
    var labels = document.getElementById('labels');
    var selectedValue = labels.options[labels.selectedIndex].value;
    const response_images = fetch('http://localhost:5000/api/images/labels/' + selectedValue)
        .then((response_images) => {
            return response_images.json();
        })
        .then((data) => {
            console.log(data);

            var counter = document.getElementById('image_counter');
            counter.innerText = 'Number of images: ' + data.length;

            document.querySelectorAll('.mySlides').forEach(e => e.remove());

            var container_images = document.getElementById('container');
            for (var j = 0; j < data.length; j++) {
                var el_images = document.createElement('div');
                el_images.className = "mySlides";
                var im = document.createElement('img');
                im.src = "data:image/jpg;base64," + data[j];
                el_images.appendChild(im);
                container_images.appendChild(el_images);
                showSlides(slideIndex);
            }
        });
}

