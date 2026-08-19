(function () {
    const form = document.getElementById("send-form");
    const result = document.getElementById("result");
    const button = document.getElementById("send-button");

    if (form) {
        form.addEventListener("submit", async function (event) {
            event.preventDefault();

            button.disabled = true;
            result.hidden = true;

            const originalText = button.textContent;
            button.textContent = "Sending...";

            try {
                const response = await fetch(form.action, {
                    method: form.method,
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: new URLSearchParams(new FormData(form)).toString()
                });

                const data = await response.json();
                result.textContent = "Result: " + (data.result ?? response.status);
                result.hidden = false;
            } catch (err) {
                result.textContent = "Result: " + err.message;
                result.hidden = false;
            } finally {
                button.disabled = false;
                button.textContent = originalText;
            }
        });
    }

    const imageForm = document.getElementById("send-image-form");
    const imageResult = document.getElementById("image-result");
    const imageButton = document.getElementById("send-image-button");

    if (imageForm) {
        imageForm.addEventListener("submit", async function (event) {
            event.preventDefault();

            imageButton.disabled = true;
            imageResult.hidden = true;

            const originalText = imageButton.textContent;
            imageButton.textContent = "Sending...";

            try {
                const response = await fetch(imageForm.action, {
                    method: imageForm.method,
                    body: new FormData(imageForm)
                });

                const data = await response.json();
                imageResult.textContent = "Result: " + (data.result ?? response.status);
                imageResult.hidden = false;
            } catch (err) {
                imageResult.textContent = "Result: " + err.message;
                imageResult.hidden = false;
            } finally {
                imageButton.disabled = false;
                imageButton.textContent = originalText;
            }
        });
    }
const metricForm = document.getElementById("send-metric-form");
    const metricResult = document.getElementById("metric-result");
    const metricButton = document.getElementById("send-metric-button");

    if (metricForm) {
        metricForm.addEventListener("submit", async function (event) {
            event.preventDefault();

            metricButton.disabled = true;
            metricResult.hidden = true;

            const originalText = metricButton.textContent;
            metricButton.textContent = "Sending...";

            try {
                const response = await fetch(metricForm.action, {
                    method: metricForm.method,
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: new URLSearchParams(new FormData(metricForm)).toString()
                });

                const data = await response.json();
                metricResult.textContent = "Result: " + (data.result ?? response.status);
                metricResult.hidden = false;
            } catch (err) {
                metricResult.textContent = "Result: " + err.message;
                metricResult.hidden = false;
            } finally {
                metricButton.disabled = false;
                metricButton.textContent = originalText;
            }
        });
    }
})();