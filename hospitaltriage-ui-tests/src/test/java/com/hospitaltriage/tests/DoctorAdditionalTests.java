package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DoctorsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Extra Doctor scenarios: validation + department filter behavior.
 */
public class DoctorAdditionalTests extends BaseUiTest {

    @Test
    public void createDoctor_missingDepartment_shouldShowError() {
        logoutIfPossible();
        loginAs("admin");

        var form = new DoctorsPage(driver).open().clickCreate();
        form.setCode(TestData.uniqueCode("DR"))
                .setFullName("BS Không chọn khoa")
                // do not select DepartmentId
                .setIsActive(true)
                .save();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Khoa") && (src.contains("bắt buộc") || src.contains("không tồn tại")),
                "Expected department required/invalid error");

        logoutIfPossible();
    }

    @Test
    public void doctors_filterByDepartment_shouldHideDoctorFromOtherDepartments() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("DRQA");
        String name = "BS Filter " + TestData.uniqueCode("");

        // Create a doctor in Khoa Nội
        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName(name)
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        DoctorsPage list = new DoctorsPage(driver);

        // Should be visible when filtering NOI
        list.open().filter(code, "NOI - Khoa Nội");
        Assert.assertTrue(list.hasDoctorCode(code), "Doctor should appear when filtering its own department");

        // Should not be visible when filtering NGOAI
        list.open().filter(code, "NGOAI - Khoa Ngoại");
        Assert.assertFalse(list.hasDoctorCode(code), "Doctor should NOT appear when filtering another department");

        // Cleanup-ish: deactivate
        list.open().filter(code, null).clickDeleteByCode(code);

        logoutIfPossible();
    }
}
