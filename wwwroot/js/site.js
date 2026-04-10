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

window.initSubjectTopicExamFilters = function (config) {
    const subjectSelect = document.querySelector(config.subjectSelector);
    const topicSelect = config.topicSelector ? document.querySelector(config.topicSelector) : null;
    const examSelect = config.examSelector ? document.querySelector(config.examSelector) : null;

    if (!subjectSelect || !topicSelect) {
        return;
    }

    const topicsUrl = config.topicsUrl;
    const examsUrl = config.examsUrl;
    const topicPlaceholder = config.topicPlaceholder ?? '-- Избери тема --';
    const examPlaceholder = config.examPlaceholder ?? '-- Избери тест --';
    const topicEmptyValue = config.topicEmptyValue ?? '';
    const examEmptyValue = config.examEmptyValue ?? '';

    const createOption = (value, text, selected) => {
        const option = document.createElement('option');
        option.value = value;
        option.textContent = text;
        option.selected = selected;
        return option;
    };

    const fillSelect = (select, items, selectedValue, placeholder, emptyValue, mapText) => {
        if (!select) {
            return;
        }

        select.innerHTML = '';
        select.appendChild(createOption(emptyValue, placeholder, !selectedValue));

        items.forEach(item => {
            const value = String(item.id);
            select.appendChild(createOption(value, mapText(item), value === selectedValue));
        });
    };

    const fetchJson = async (url) => {
        const response = await fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!response.ok) {
            throw new Error('Неуспешно зареждане на данните.');
        }

        return await response.json();
    };

    const loadTopics = async (selectedTopicValue) => {
        const subjectId = subjectSelect.value;
        const url = new URL(topicsUrl, window.location.origin);

        if (subjectId) {
            url.searchParams.set('subjectId', subjectId);
        }

        const topics = await fetchJson(url);
        const topicExists = topics.some(topic => String(topic.id) === String(selectedTopicValue));

        fillSelect(
            topicSelect,
            topics,
            topicExists ? String(selectedTopicValue) : '',
            topicPlaceholder,
            topicEmptyValue,
            topic => topic.name
        );
    };

    const loadExams = async (selectedExamValue) => {
        if (!examSelect || !examsUrl) {
            return;
        }

        const url = new URL(examsUrl, window.location.origin);

        if (subjectSelect.value) {
            url.searchParams.set('subjectId', subjectSelect.value);
        }

        if (topicSelect.value) {
            url.searchParams.set('topicId', topicSelect.value);
        }

        const exams = await fetchJson(url);
        const examExists = exams.some(exam => String(exam.id) === String(selectedExamValue));

        fillSelect(
            examSelect,
            exams,
            examExists ? String(selectedExamValue) : '',
            examPlaceholder,
            examEmptyValue,
            exam => exam.title
        );
    };

    const initialize = async () => {
        const selectedTopicValue = topicSelect.dataset.selectedValue || topicSelect.value;
        const selectedExamValue = examSelect?.dataset.selectedValue || examSelect?.value || '';

        try {
            await loadTopics(selectedTopicValue);
            await loadExams(selectedExamValue);
        } catch (error) {
            console.error(error);
        }
    };

    subjectSelect.addEventListener('change', async () => {
        topicSelect.dataset.selectedValue = '';
        if (examSelect) {
            examSelect.dataset.selectedValue = '';
        }

        try {
            await loadTopics('');
            await loadExams('');
        } catch (error) {
            console.error(error);
        }
    });

    topicSelect.addEventListener('change', async () => {
        topicSelect.dataset.selectedValue = topicSelect.value;

        if (!examSelect) {
            return;
        }

        examSelect.dataset.selectedValue = '';

        try {
            await loadExams('');
        } catch (error) {
            console.error(error);
        }
    });

    initialize();
};
