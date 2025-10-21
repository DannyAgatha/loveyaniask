const envoltura = document.querySelector(".envoltura-sobre");
const carta = document.querySelector(".carta");
const hayirBtn = document.getElementById('hayir-btn');
const hayirError = document.getElementById('hayir-error');
const butonlar = document.getElementById('butonlar');

// Zarf ve kalp açma/kapama
document.addEventListener("click", (e) => {
    // HAYIR'a tıklanırsa mektup açılmasın
    if (e.target.matches("#hayir-btn")) {
        return;
    }
    // Zarf veya kalbe tıklandığında aç/kapat
    if (e.target.matches(".sobre") || 
        e.target.matches(".solapa-derecha") ||
        e.target.matches(".solapa-izquierda") ||
        e.target.matches(".corazon")) {
        envoltura.classList.toggle("abierto");
    } else if (e.target.matches(".sobre *")) {
        if (!carta.classList.contains("abierta")) {
            carta.classList.add("mostrar-carta");
            setTimeout(() => {
                carta.classList.remove("mostrar-carta");
                carta.classList.add("abierta");
            }, 500);
            envoltura.classList.add("desactivar-sobre")
        } else {
            carta.classList.add("cerrando-carta");
            envoltura.classList.remove("desactivar-sobre");
            setTimeout(() => {
                carta.classList.remove("cerrando-carta")
                carta.classList.remove("abierta")
            }, 500);
        }
    } 
});

// Hayır butonu animasyonu için
hayirBtn.addEventListener('click', function(e) {
    e.preventDefault();
    // Butonları gizle
    butonlar.style.opacity = '0';
    setTimeout(() => {
        butonlar.style.display = 'none';
        // Hata mesajını göster
        hayirError.style.display = 'block';
        hayirError.classList.remove('hayir-error');
        void hayirError.offsetWidth; // reflow
        hayirError.classList.add('hayir-error');
        hayirError.style.opacity = '1';
        // 2.1s sonra tekrar butonları göster
        setTimeout(() => {
            hayirError.style.opacity = '0';
            setTimeout(() => {
                hayirError.style.display = 'none';
                butonlar.style.display = 'flex';
                setTimeout(() => {
                    butonlar.style.opacity = '1';
                }, 10);
            }, 350);
        }, 2100);
    }, 200);
});