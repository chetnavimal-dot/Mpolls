window.renderSurvey = (surveyJson, elementId = "surveyContainer", dotNetRef) => {
    const container = document.getElementById(elementId);
    if (!container) {
        return;
    }

    if (!surveyJson) {
        container.innerHTML = "";
        return;
    }

    if (typeof surveyJson === "string") {
        try {
            surveyJson = JSON.parse(surveyJson);
        } catch (error) {
            console.error("Failed to parse survey definition", error);
            container.innerHTML = "";
            return;
        }
    }

    container.innerHTML = "";
    if (!surveyJson.questionsOnPageMode) {
        surveyJson.questionsOnPageMode = "questionPerPage";
    }
    surveyJson.pages?.forEach(page => {
        page.elements?.forEach(el => {
            if (el.type === "slider") {
                el.type = "rating";
            }

            if (el.type === "checkbox" || el.type === "radiogroup") {
                if (!el.colCount || el.colCount < 2) {
                    el.colCount = 2;
                }
            }
        });
    });
    const survey = new Survey.Model(surveyJson);

    survey.onComplete.add((sender) => {
        const results = sender.data;
        const jsonResults = JSON.stringify(results);

        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("OnSurveyComplete", jsonResults);
            return;
        }

        const encoded = encodeURIComponent(jsonResults);
        window.location.assign(`/survey-results?payload=${encoded}`);
    });

    survey.render(container);
};
