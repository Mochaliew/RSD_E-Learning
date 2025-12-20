// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/* ----------------------------- Course Script ----------------------------- */

let materialCount = 1;

function addMaterial() {
    materialCount++;
    const container = document.getElementById('materialsContainer');
    const newMaterial = document.createElement('div');
    newMaterial.className = 'material-item';
    newMaterial.innerHTML = `
                <button type="button" class="remove-btn" onclick="removeMaterial(this)">Remove</button>
                <div class="form-group">
                    <label>Material Type</label>
                    <select name="materialType[]" class="material-type">
                        <option value="pdf">PDF Document</option>
                        <option value="video">Video</option>
                        <option value="text">Text Lesson</option>
                        <option value="code">Code Demo</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>Material Title</label>
                    <input type="text" name="materialTitle[]" placeholder="Enter material title">
                </div>
                <div class="form-group">
                    <label>Upload File</label>
                    <input type="file" name="materialFile[]">
                </div>
            `;
    container.appendChild(newMaterial);
}

function removeMaterial(button) {
    button.parentElement.remove();
}

// Form validation
const courseForm = document.getElementById('courseForm');
if (courseForm) {
    courseForm.addEventListener('submit', function (e) {
        const title = document.getElementById('courseTitle').value;
        const category = document.getElementById('category').value;
        const description = document.getElementById('description').value;
        const difficulty = document.getElementById('difficulty').value;

        if (!title || !category || !description || !difficulty) {
            e.preventDefault();
            alert('Please fill in all required fields marked with *');
        }
    });
}

/* ----------------------------- Assessment Script ----------------------------- */

let currentStep = 1;
let questionCount = 1;

function nextStep(step) {
    // Validate current step
    if (step === 2 && !validateStep1()) {
        alert('Please fill in all required fields');
        return;
    }
    if (step === 3 && !validateStep2()) {
        alert('Please add at least one complete question with all options and mark the correct answer');
        return;
    }

    // Hide current step
    document.getElementById('step' + currentStep).classList.remove('active');
    document.getElementById('step' + currentStep + 'Circle').classList.remove('active');
    document.getElementById('step' + currentStep + 'Circle').classList.add('completed');

    // Show next step
    currentStep = step;
    document.getElementById('step' + currentStep).classList.add('active');
    document.getElementById('step' + currentStep + 'Circle').classList.add('active');

    // If moving to preview, populate preview
    if (step === 3) {
        populatePreview();
    }

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function prevStep(step) {
    document.getElementById('step' + currentStep).classList.remove('active');
    document.getElementById('step' + currentStep + 'Circle').classList.remove('active');

    currentStep = step;
    document.getElementById('step' + currentStep).classList.add('active');
    document.getElementById('step' + currentStep + 'Circle').classList.add('active');
    document.getElementById('step' + currentStep + 'Circle').classList.remove('completed');

    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function validateStep1() {
    const title = document.getElementById('assessmentTitle').value;
    const passing = document.getElementById('passingMark').value;
    return title && passing;
}

function validateStep2() {
    const questions = document.querySelectorAll('.question-container');
    if (questions.length === 0) return false;

    for (let question of questions) {
        const questionText = question.querySelector('.question-text').value;
        const options = question.querySelectorAll('.option-text');
        const hasCorrect = question.querySelector('input[type="radio"]:checked');

        if (!questionText || !hasCorrect) return false;

        for (let option of options) {
            if (!option.value) return false;
        }
    }
    return true;
}

function addQuestion() {
    questionCount++;
    const container = document.getElementById('questionsContainer');
    const newQuestion = document.createElement('div');
    newQuestion.className = 'question-container';
    newQuestion.innerHTML = `
                <div class="question-header">
                    <span class="question-number">Question ${questionCount}</span>
                    <button type="button" class="remove-question-btn" onclick="removeQuestion(this)">Remove</button>
                </div>
                <div class="form-group">
                    <label>Question Text *</label>
                    <textarea class="question-text" rows="3" required placeholder="Enter your question"></textarea>
                </div>
                <div class="options-group">
                    <label>Answer Options:</label>
                    <div class="option-item">
                        <span class="option-label">Option A:</span>
                        <input type="text" class="option-text" placeholder="Enter option A" required>
                        <input type="radio" name="correct-${questionCount}" value="A" required>
                        <label>Correct</label>
                    </div>
                    <div class="option-item">
                        <span class="option-label">Option B:</span>
                        <input type="text" class="option-text" placeholder="Enter option B" required>
                        <input type="radio" name="correct-${questionCount}" value="B">
                        <label>Correct</label>
                    </div>
                    <div class="option-item">
                        <span class="option-label">Option C:</span>
                        <input type="text" class="option-text" placeholder="Enter option C" required>
                        <input type="radio" name="correct-${questionCount}" value="C">
                        <label>Correct</label>
                    </div>
                    <div class="option-item">
                        <span class="option-label">Option D:</span>
                        <input type="text" class="option-text" placeholder="Enter option D" required>
                        <input type="radio" name="correct-${questionCount}" value="D">
                        <label>Correct</label>
                    </div>
                </div>
            `;
    container.appendChild(newQuestion);
}

function removeQuestion(button) {
    if (document.querySelectorAll('.question-container').length > 1) {
        button.closest('.question-container').remove();
        updateQuestionNumbers();
    } else {
        alert('You must have at least one question');
    }
}

function updateQuestionNumbers() {
    const questions = document.querySelectorAll('.question-container');
    questions.forEach((q, index) => {
        q.querySelector('.question-number').textContent = `Question ${index + 1}`;
    });
    questionCount = questions.length;
}

function populatePreview() {
    // Basic info
    document.getElementById('previewTitle').textContent = document.getElementById('assessmentTitle').value;
    document.getElementById('previewPassing').textContent = document.getElementById('passingMark').value + '%';

    // Questions
    const questions = document.querySelectorAll('.question-container');
    document.getElementById('previewQuestions').textContent = questions.length;

    const previewContainer = document.getElementById('previewQuestionsContainer');
    previewContainer.innerHTML = '';

    questions.forEach((q, index) => {
        const questionText = q.querySelector('.question-text').value;
        const options = q.querySelectorAll('.option-text');
        const correctAnswer = q.querySelector('input[type="radio"]:checked').value;

        const previewQ = document.createElement('div');
        previewQ.className = 'preview-question';
        previewQ.innerHTML = `
                    <div class="preview-question-text">${index + 1}. ${questionText}</div>
                    ${Array.from(options).map((opt, i) => {
            const letter = String.fromCharCode(65 + i);
            const isCorrect = letter === correctAnswer;
            return `<div class="preview-option ${isCorrect ? 'correct' : ''}">
                            ${letter}. ${opt.value} ${isCorrect ? '✓' : ''}
                        </div>`;
        }).join('')}
                `;
        previewContainer.appendChild(previewQ);
    });
}

async function publishAssessment() {

    const courseId = document.getElementById("courseId").value;
    const title = document.getElementById("assessmentTitle").value;
    const deadlineHours = parseInt(document.getElementById("deadline").value);

    if (!deadlineHours || deadlineHours < 1) {
        alert("Please enter a valid deadline (at least 1 hour).");
        return;
    }

    // Convert hours to DateTime
    const deadlineDate = new Date();
    deadlineDate.setHours(deadlineDate.getHours() + deadlineHours);

    const questions = [];

    document.querySelectorAll(".question-container").forEach(q => {
        const questionText = q.querySelector(".question-text").value;
        const options = q.querySelectorAll(".option-text");
        const correct = q.querySelector("input[type='radio']:checked")?.value;

        questions.push({
            QuestionDetail: questionText,
            AnswerA: options[0].value,
            AnswerB: options[1].value,
            AnswerC: options[2].value,
            AnswerD: options[3].value,
            CorrectAnswer: correct
        });
    });

    if (questions.length === 0) {
        alert("Please add at least one question.");
        return;
    }
    const passingMark = parseInt(document.getElementById("passingMark").value);


    const payload = {
        CourseId: parseInt(courseId),
        Title: title,
        PassingMark: passingMark,
        DeadLine: deadlineDate.toISOString(),
        Questions: questions
    };


    try {
        const response = await fetch("/api/teacher/create-assessment", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const error = await response.text();
            alert("Failed to save assessment: " + error);
            return;
        }

        document.getElementById("successMessage").style.display = "block";

        setTimeout(() => {
            window.location.href = "/Teacher/TeacherIndex";
        }, 1500);

    } catch (err) {
        console.error(err);
        alert("Server error while saving assessment.");
    }
}
