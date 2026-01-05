package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import org.testng.Assert;
import org.testng.annotations.DataProvider;
import org.testng.annotations.Test;

/**
 * Access control checks specifically for "Create" pages.
 *
 * Rationale: many systems allow listing but restrict create actions.
 * This suite verifies direct URL access for create pages matches controller attributes.
 */
public class CreatePagesAccessControlTests extends BaseUiTest {

    @DataProvider(name = "createPageMatrix")
    public Object[][] createPageMatrix() {
        // role, relativeUrl, shouldAllow, markerTextOnPage
        return new Object[][]{
                // Departments.Create (Manager/Admin)
                {"admin", "Departments/Create", true, "Tạo khoa"},
                {"manager", "Departments/Create", true, "Tạo khoa"},
                {"receptionist", "Departments/Create", false, ""},
                {"nurse", "Departments/Create", false, ""},
                {"doctor1", "Departments/Create", false, ""},

                // Doctors.Create (Manager/Admin)
                {"admin", "Doctors/Create", true, "Tạo bác sĩ"},
                {"manager", "Doctors/Create", true, "Tạo bác sĩ"},
                {"receptionist", "Doctors/Create", false, ""},
                {"nurse", "Doctors/Create", false, ""},
                {"doctor1", "Doctors/Create", false, ""},

                // Intake/Register (Receptionist/Manager/Admin)
                {"admin", "Intake/Register", true, "Tiếp nhận bệnh nhân"},
                {"manager", "Intake/Register", true, "Tiếp nhận bệnh nhân"},
                {"receptionist", "Intake/Register", true, "Tiếp nhận bệnh nhân"},
                {"nurse", "Intake/Register", false, ""},
                {"doctor1", "Intake/Register", false, ""},
        };
    }

    @Test(dataProvider = "createPageMatrix")
    public void rbac_createPages_directUrlAccess_matchesExpected(String roleKey, String relativeUrl, boolean shouldAllow, String markerText) {
        logoutIfPossible();
        loginAs(roleKey);

        driver.get(Config.baseUrl() + relativeUrl);

        if (shouldAllow) {
            Assert.assertFalse(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                    "Expected access allowed, but was denied. role=" + roleKey + ", url=" + relativeUrl);
            Assert.assertTrue(driver.getPageSource().contains(markerText),
                    "Expected marker text not found. role=" + roleKey + ", url=" + relativeUrl);
        } else {
            Assert.assertTrue(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                    "Expected AccessDenied. role=" + roleKey + ", url=" + relativeUrl + ", actual=" + driver.getCurrentUrl());
            Assert.assertTrue(driver.getPageSource().contains("Bạn không có quyền"),
                    "AccessDenied message not found. role=" + roleKey + ", url=" + relativeUrl);
        }

        logoutIfPossible();
    }

    @Test
    public void createPages_whenLoggedOut_shouldRedirectToLogin_withReturnUrl() {
        logoutIfPossible();

        String[] createUrls = new String[]{
                "Departments/Create",
                "Doctors/Create",
                "Intake/Register"
        };

        for (String u : createUrls) {
            driver.get(Config.baseUrl() + u);

            String current = driver.getCurrentUrl();
            Assert.assertTrue(current.contains("/Identity/Account/Login"),
                    "Expected redirect to Identity Login when logged out. url=" + u + ", actual=" + current);
            Assert.assertTrue(current.contains("returnUrl="),
                    "Expected returnUrl query parameter. url=" + u + ", actual=" + current);
        }
    }
}
