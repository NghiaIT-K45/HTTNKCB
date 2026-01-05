package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.LoginPage;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Negative / edge tests around the ASP.NET Core Identity login.
 */
public class AuthNegativeTests extends BaseUiTest {

    @Test
    public void login_invalidPassword_staysOnLoginPage_andShowsError() {
        logoutIfPossible();

        String email = Config.credential("admin", "email");

        LoginPage lp = new LoginPage(driver)
                .open()
                .typeEmail(email)
                .typePassword("WRONG_PASSWORD")
                .submitExpectingError();

        Assert.assertTrue(driver.getCurrentUrl().contains("/Identity/Account/Login"),
                "Should remain on Login page when credentials are invalid");
        Assert.assertTrue(lp.hasErrorMessage(), "Expected an error message for invalid login");
    }

    @Test
    public void login_emptyEmailPassword_showsValidationErrors() {
        logoutIfPossible();

        LoginPage lp = new LoginPage(driver)
                .open()
                .typeEmail("")
                .typePassword("")
                .submitExpectingError();

        Assert.assertTrue(driver.getCurrentUrl().contains("/Identity/Account/Login"),
                "Should remain on Login page when required fields are missing");
        Assert.assertTrue(lp.hasErrorMessage(), "Expected validation errors on Login page");
    }

    @Test
    public void login_redirectsToReturnUrl_afterLogin() {
        logoutIfPossible();

        // Trigger RequireLogin filter so we land on Login with returnUrl
        driver.get(Config.baseUrl() + "Departments");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Identity/Account/Login"), "Should redirect to login");
        Assert.assertTrue(driver.getCurrentUrl().contains("returnUrl="), "Should contain returnUrl parameter");

        // Log in and expect to land back on /Departments
        new LoginPage(driver)
                .typeEmail(Config.credential("admin", "email"))
                .typePassword(Config.credential("admin", "password"))
                .submit();

        Assert.assertTrue(driver.getCurrentUrl().contains("/Departments"),
                "After successful login, should return to the protected page (/Departments)");
    }
}
