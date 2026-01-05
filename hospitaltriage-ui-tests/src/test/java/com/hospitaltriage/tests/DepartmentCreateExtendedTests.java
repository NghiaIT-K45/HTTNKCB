package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DepartmentsPage;
import com.hospitaltriage.pages.DepartmentFormPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Extra "Create New" test cases for Departments to increase coverage:
 *  - positive: max length boundaries, inactive create, manager role
 *  - negative: code too long, trimming behavior
 */
public class DepartmentCreateExtendedTests extends BaseUiTest {

    @Test
    public void createDepartment_success_codeMax20_nameMax200_shouldSucceed() {
        logoutIfPossible();
        loginAs("admin");

        // uniqueCode(prefix) = prefix + 14-digit timestamp + 3-digit random
        // prefix len 3 => total len 20 (matches [StringLength(20)])
        String code = TestData.uniqueCode("DEP");
        Assert.assertEquals(code.length(), 20, "Sanity: code must be exactly 20 chars");

        String name200 = "N".repeat(200);

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName(name200)
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        // Expect redirect + flash message
        Assert.assertTrue(driver.getPageSource().contains("Tạo khoa thành công"),
                "Expected success flash message after create");

        DepartmentsPage list = new DepartmentsPage(driver).open().search(code);
        Assert.assertTrue(list.hasDepartmentCode(code), "Created department should appear in list");

        // cleanup-ish (deactivate)
        list.clickDeleteByCode(code);
        logoutIfPossible();
    }

    @Test
    public void createDepartment_success_inactiveDepartment_shouldShowInactiveBadge() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("IN" );

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName("Khoa Inactive " + code)
                .setIsActive(false)
                .setIsGeneral(false)
                .save();

        DepartmentsPage list = new DepartmentsPage(driver).open().search(code);
        Assert.assertTrue(list.hasDepartmentCode(code), "Created department should appear in list");
        Assert.assertTrue(list.rowHasBadge(code, "Inactive"), "Inactive badge should be shown");

        logoutIfPossible();
    }

    @Test
    public void createDepartment_negative_codeTooLong_shouldStayOnForm_andShowValidationHint() {
        logoutIfPossible();
        loginAs("admin");

        // prefix len 4 => total len 21 (> 20)
        String code21 = TestData.uniqueCode("LONG");
        Assert.assertTrue(code21.length() > 20, "Sanity: code must be > 20 chars");

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                // force to bypass any client-side trimming/constraints
                .forceCode(code21)
                .setName("Khoa code too long")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tạo khoa"), "Should remain on Create Department page");

        String summary = new DepartmentFormPage(driver).validationSummaryText();
        Assert.assertTrue(summary.contains("20") || src.contains("20") || src.contains("maximum"),
                "Expected max-length validation hint for Code (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void createDepartment_trimInputs_shouldCreateWithTrimmedCode() {
        logoutIfPossible();
        loginAs("admin");

        String trimmedCode = TestData.uniqueCode("TRM");
        String rawCode = "  " + trimmedCode + "  ";

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(rawCode)
                .setName("  Khoa Trimmed  ")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        DepartmentsPage list = new DepartmentsPage(driver).open().search(trimmedCode);
        Assert.assertTrue(list.hasDepartmentCode(trimmedCode), "Department should be searchable by trimmed code");

        // cleanup-ish
        list.clickDeleteByCode(trimmedCode);
        logoutIfPossible();
    }

    @Test
    public void createDepartment_asManager_shouldSucceed() {
        logoutIfPossible();
        loginAs("manager");

        String code = TestData.uniqueCode("MGR");

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName("Khoa Manager tạo " + code)
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        Assert.assertFalse(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                "Manager should be allowed to create departments");
        Assert.assertTrue(driver.getPageSource().contains("Tạo khoa thành công")
                        || new DepartmentsPage(driver).open().search(code).hasDepartmentCode(code),
                "Expected manager create success");

        // cleanup-ish
        new DepartmentsPage(driver).open().search(code).clickDeleteByCode(code);
        logoutIfPossible();
    }
}
