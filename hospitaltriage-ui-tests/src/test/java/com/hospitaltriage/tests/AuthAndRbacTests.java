package com.hospitaltriage.tests;

import com.hospitaltriage.pages.HomePage;
import org.testng.Assert;
import org.testng.annotations.DataProvider;
import org.testng.annotations.Test;

public class AuthAndRbacTests extends BaseUiTest {

    @Test
    public void protectedPage_requiresLogin_redirectsToIdentityLogin() {
        logoutIfPossible();
        driver.get(com.hospitaltriage.config.Config.baseUrl() + "Dashboard");
        String url = driver.getCurrentUrl();
        Assert.assertTrue(url.contains("/Identity/Account/Login"), "Should redirect to login, actual: " + url);
        Assert.assertTrue(url.contains("returnUrl="), "Should include returnUrl param, actual: " + url);
    }

    @DataProvider
    public Object[][] rolesAndNavExpectations() {
        // roleKey, dashboard, intake, triage, reports, catalog
        return new Object[][]{
                {"admin", true, true, true, true, true},
                {"manager", true, true, false, true, true},
                {"receptionist", false, true, false, false, false},
                {"nurse", true, false, true, false, false},
                {"doctor1", true, false, true, false, false},
        };
    }

    @Test(dataProvider = "rolesAndNavExpectations")
    public void navbar_visibility_matchesRole(String roleKey,
                                              boolean expDashboard,
                                              boolean expIntake,
                                              boolean expTriage,
                                              boolean expReports,
                                              boolean expCatalog) {
        logoutIfPossible();
        HomePage home = loginAs(roleKey).open();

        var nav = home.navbar();
        Assert.assertEquals(nav.isLinkVisible("Dashboard"), expDashboard, "Dashboard menu mismatch for " + roleKey);
        Assert.assertEquals(nav.isLinkVisible("Tiếp nhận"), expIntake, "Intake menu mismatch for " + roleKey);
        Assert.assertEquals(nav.isLinkVisible("Phân luồng"), expTriage, "Triage menu mismatch for " + roleKey);
        Assert.assertEquals(nav.isLinkVisible("Báo cáo"), expReports, "Reports menu mismatch for " + roleKey);
        Assert.assertEquals(nav.isLinkVisible("Danh mục"), expCatalog, "Catalog menu mismatch for " + roleKey);

        logoutIfPossible();
    }

    @Test
    public void receptionist_cannot_access_catalog_departments() {
        logoutIfPossible();
        loginAs("receptionist");

        driver.get(com.hospitaltriage.config.Config.baseUrl() + "Departments");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                "Should be redirected to /Home/AccessDenied, actual: " + driver.getCurrentUrl());
        Assert.assertTrue(driver.getPageSource().contains("Bạn không có quyền"), "AccessDenied message not found");

        logoutIfPossible();
    }

    @Test
    public void manager_menu_doesNotShowTriage_but_urlIsAccessible_byCurrentCode() {
        // NOTE: Current code allows Manager to access /Triage (controller has RequireRole(..., Manager, ...))
        // but menu doesn't show it. This test documents current behavior.
        logoutIfPossible();
        HomePage home = loginAs("manager").open();
        Assert.assertFalse(home.navbar().isLinkVisible("Phân luồng"), "Manager should not see triage menu");

        driver.get(com.hospitaltriage.config.Config.baseUrl() + "Triage");
        // UI text updated in new layout
        Assert.assertTrue(driver.getPageSource().contains("đang chờ phân luồng")
                        || driver.getPageSource().contains("Phân luồng"),
                "Triage page content not found");

        logoutIfPossible();
    }
}
