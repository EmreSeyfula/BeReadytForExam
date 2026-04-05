document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});


document.querySelectorAll('form').forEach(form => {
    form.addEventListener('submit', function () {
        const submitBtn = this.querySelector('button[type="submit"]');
        if (submitBtn && !submitBtn.disabled) {
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Зареждане...';
            submitBtn.disabled = true;

            
            setTimeout(() => {
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }, 30000);
        }
    });
});

const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
});

document.querySelectorAll('.alert').forEach(alert => {
    setTimeout(() => {
        alert.style.transition = 'opacity 0.5s ease';
        alert.style.opacity = '0';
        setTimeout(() => alert.remove(), 500);
    }, 5000);
});

const animateProgressBars = () => {
    document.querySelectorAll('.progress-bar').forEach(bar => {
        const width = bar.style.width;
        bar.style.width = '0';
        setTimeout(() => {
            bar.style.width = width;
        }, 100);
    });
};

document.addEventListener('DOMContentLoaded', () => {
    animateProgressBars();

    // Add floating animation to hero icons
    const heroIcon = document.querySelector('.hero-section .fa-chalkboard-teacher');
    if (heroIcon) {
        let float = 0;
        setInterval(() => {
            float = float === 0 ? 10 : 0;
            heroIcon.style.transform = `translateY(${float}px)`;
        }, 2000);
    }
});

function addQuestion() {
    const container = document.getElementById('questions-container');
    const index = container.children.length;

    const html = `
        <div class="card mb-3 question-block">
            <div class="card-body">
                <label>Въпрос</label>
                <input name="Questions[${index}].Text" class="form-control mb-2" />

                <div>
                    <label>Отговори:</label>

                    <div class="d-flex gap-2 mb-2">
                        <input name="Questions[${index}].Options[0].Text" class="form-control" />
                        <input type="checkbox" name="Questions[${index}].Options[0].IsCorrect" />
                    </div>

                    <div class="d-flex gap-2 mb-2">
                        <input name="Questions[${index}].Options[1].Text" class="form-control" />
                        <input type="checkbox" name="Questions[${index}].Options[1].IsCorrect" />
                    </div>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', html);
}