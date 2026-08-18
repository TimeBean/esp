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
})();