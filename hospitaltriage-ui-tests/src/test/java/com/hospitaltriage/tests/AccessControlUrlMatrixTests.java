package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import org.testng.Assert;
import org.testng.annotations.DataProvider;
import org.testng.annotations.Test;

/**
 * Role-based access control (RBAC) matrix:
 * verifies that direct URL access matches the [RequireRole] attributes.
 */
public class AccessControlUrlMatrixTests extends BaseUiTest {

    @DataProvider(name = "rbacMatrix")
    public Object[][] rbacMatrix() {
        // role, relativeUrl, shouldAllow, markerTextOnPage
        return new Object[][]{
                // Admin
                {"admin", "Dashboard", true, "Dashboard"},
                {"admin", "Intake/Register", true, "Tiếp nhận bệnh nhân"},
                {"admin", "Triage", true, "Danh sách chờ phân luồng"},
                {"admin", "Reports", true, "Báo cáo"},
                {"admin", "Departments", true, "Khoa khám"},
                {"admin", "Doctors", true, "Bác sĩ"},

                // Manager
                {"manager", "Dashboard", true, "Dashboard"},
                {"manager", "Intake/Register", true, "Tiếp nhận bệnh nhân"},
                {"manager", "Triage", true, "Danh sách chờ phân luồng"},
                {"manager", "Reports", true, "Báo cáo"},
                {"manager", "Departments", true, "Khoa khám"},
                {"manager", "Doctors", true, "Bác sĩ"},

                // Receptionist
                {"receptionist", "Dashboard", false, ""},
                {"receptionist", "Intake/Register", true, "Tiếp nhận bệnh nhân"},
                {"receptionist", "Triage", false, ""},
                {"receptionist", "Reports", false, ""},
                {"receptionist", "Departments", false, ""},
                {"receptionist", "Doctors", false, ""},

                // Nurse
                {"nurse", "Dashboard", true, "Dashboard"},
                {"nurse", "Intake/Register", false, ""},
                {"nurse", "Triage", true, "Danh sách chờ phân luồng"},
                {"nurse", "Reports", false, ""},
                {"nurse", "Departments", false, ""},
                {"nurse", "Doctors", false, ""},

                // Doctor
                {"doctor1", "Dashboard", true, "Dashboard"},
                {"doctor1", "Intake/Register", false, ""},
                {"doctor1", "Triage", true, "Danh sách chờ phân luồng"},
                {"doctor1", "Reports", false, ""},
                {"doctor1", "Departments", false, ""},
                {"doctor1", "Doctors", false, ""},
        };
    }

    @Test(dataProvider = "rbacMatrix")
    public void rbac_directUrlAccess_matchesExpected(String roleKey, String relativeUrl, boolean shouldAllow, String markerText) {
        logoutIfPossible();
        loginAs(roleKey);

        driver.get(Config.baseUrl() + relativeUrl);

        if (shouldAllow) {
            Assert.assertFalse(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                    "Expected access allowed, but was denied. role=" + roleKey + ", url=" + relativeUrl);
            Assert.assertFalse(driver.getCurrentUrl().contains("/Identity/Account/Login"),
                    "Expected already logged in, should not redirect to login. role=" + roleKey + ", url=" + relativeUrl);
            Assert.assertTrue(driver.getPageSource().contains(markerText),
                    "Expected marker text not found. role=" + roleKey + ", url=" + relativeUrl + ", marker=" + markerText);
        } else {
            Assert.assertTrue(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                    "Expected AccessDenied. role=" + roleKey + ", url=" + relativeUrl + ", actual=" + driver.getCurrentUrl());
            Assert.assertTrue(driver.getPageSource().contains("Bạn không có quyền"),
                    "AccessDenied message not found. role=" + roleKey + ", url=" + relativeUrl);
        }

        logoutIfPossible();
    }
}
