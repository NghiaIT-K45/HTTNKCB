package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DepartmentsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Extra Department scenarios (negative + search) to increase coverage.
 */
public class DepartmentAdditionalTests extends BaseUiTest {

    @Test
    public void createDepartment_requiredNameValidation() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("D");

        var form = new DepartmentsPage(driver).open().clickCreate();
        form.setCode(code)
                .setName("")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Name là bắt buộc"),
                "Should show required Name validation");

        logoutIfPossible();
    }

    @Test
    public void createDepartment_codeMaxLength_shouldShowValidationAndStayOnCreate() {
        logoutIfPossible();
        loginAs("admin");

        String longCode = "X".repeat(25); // > 20

        var form = new DepartmentsPage(driver).open().clickCreate();
        form.setCode(longCode)
                .setName("Khoa Code dài")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        // We don't hardcode the exact StringLength message because it can be locale-dependent.
        Assert.assertTrue(driver.getPageSource().contains("Tạo khoa"),
                "Should remain on Create Department page when validation fails");
        Assert.assertTrue(driver.getPageSource().contains("maximum") || driver.getPageSource().contains("20") || driver.getPageSource().contains("StringLength"),
                "Expected some max-length validation hint (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void departments_searchByName_shouldReturnCreatedDepartment() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("S");
        String name = "Khoa Search " + TestData.uniqueCode("");

        // Create
        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName(name)
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        // Search by name keyword
        DepartmentsPage list = new DepartmentsPage(driver).open().search("Search");
        Assert.assertTrue(list.hasDepartmentCode(code),
                "Searching by name should return the created department");

        // Cleanup-ish: deactivate to avoid polluting list
        list.search(code).clickDeleteByCode(code);

        logoutIfPossible();
    }
}
