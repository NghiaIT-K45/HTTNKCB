package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.HomePage;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Session-level checks: logout, back-navigation, ...
 */
public class SessionAndLogoutTests extends BaseUiTest {

    @Test
    public void logout_shouldTerminateSession_protectedPagesRequireLoginAgain() {
        logoutIfPossible();
        HomePage home = loginAs("admin").open();

        // Sanity: can access a protected page while logged in
        driver.get(Config.baseUrl() + "Departments");
        Assert.assertTrue(driver.getPageSource().contains("Khoa khám"), "Should access Departments while logged in");

        // Logout
        home.navbar().logout();
        Assert.assertTrue(home.navbar().isLinkVisible("Login"), "Login button should be visible after logout");

        // Now protected page should redirect to login
        driver.get(Config.baseUrl() + "Departments");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Identity/Account/Login"),
                "After logout, protected pages should redirect to Login");
    }

    @Test
    public void backAfterLogout_shouldNotRestoreAuthenticatedSession() {
        logoutIfPossible();
        HomePage home = loginAs("admin").open();

        driver.get(Config.baseUrl() + "Dashboard");
        Assert.assertTrue(driver.getPageSource().contains("Dashboard"), "Should reach Dashboard");

        home.navbar().logout();
        Assert.assertTrue(home.navbar().isLinkVisible("Login"), "Login should be visible after logout");

        // Try to go back (browser history)
        driver.navigate().back();

        // Should still be unauthenticated: either sees login or access denied redirect
        String url = driver.getCurrentUrl();
        Assert.assertTrue(url.contains("/Identity/Account/Login") || url.contains("/Home/AccessDenied") || url.endsWith("/"),
                "Back navigation should not restore session. url=" + url);
    }
}
