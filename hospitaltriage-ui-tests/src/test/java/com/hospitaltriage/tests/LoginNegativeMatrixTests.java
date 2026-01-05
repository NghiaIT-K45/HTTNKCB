package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.LoginPage;
import org.testng.Assert;
import org.testng.annotations.DataProvider;
import org.testng.annotations.Test;

/**
 * A more exhaustive negative test matrix for the Identity login.
 *
 * We keep assertions robust:
 *  - Either server-side validation/errors are visible, OR
 *  - HTML5 validation blocks form submission (email format), which we detect via validationMessage.
 */
public class LoginNegativeMatrixTests extends BaseUiTest {

    @DataProvider(name = "invalidLoginCases")
    public Object[][] invalidLoginCases() {
        String adminEmail = Config.credential("admin", "email");
        String adminPass = Config.credential("admin", "password");

        return new Object[][]{
                {"EMPTY_BOTH", "", ""},
                {"EMPTY_EMAIL", "", adminPass},
                {"EMPTY_PASSWORD", adminEmail, ""},

                // HTML5 email type may block obvious invalid formats (e.g. "abc")
                {"EMAIL_INVALID_FORMAT", "abc", "whatever"},

                // Valid-looking email but not existing account
                {"NON_EXISTING_EMAIL", "noone@hospital.local", "whatever"},

                // Wrong password
                {"WRONG_PASSWORD", adminEmail, "WRONG_PASSWORD"},
                {"PASSWORD_TRAILING_SPACE", adminEmail, adminPass + " "},

                // Common hostile strings (should not crash; should remain on login)
                {"SQLI_LIKE", "a@b.com' OR 1=1 --", "whatever"},
                {"XSS_LIKE", "<script>alert(1)</script>@a.com", "whatever"},
        };
    }

    @Test(dataProvider = "invalidLoginCases")
    public void login_invalidInputs_shouldNotLogin(String caseId, String email, String password) {
        logoutIfPossible();

        LoginPage lp = new LoginPage(driver)
                .open()
                .typeEmail(email)
                .typePassword(password)
                .submitExpectingError();

        Assert.assertTrue(driver.getCurrentUrl().contains("/Identity/Account/Login"),
                "Should remain on Login page. case=" + caseId + ", url=" + driver.getCurrentUrl());

        // Either server-side error message exists, or HTML5 validationMessage (email) is not empty.
        boolean hasServerError = lp.hasErrorMessage();
        boolean hasHtml5 = lp.emailHtml5ValidationMessage() != null && !lp.emailHtml5ValidationMessage().isBlank();

        Assert.assertTrue(hasServerError || hasHtml5,
                "Expected server-side error OR HTML5 validation message. case=" + caseId +
                        ", serverError='" + lp.errorText() + "', html5='" + lp.emailHtml5ValidationMessage() + "'");
    }
}
