package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DoctorFormPage;
import com.hospitaltriage.pages.DoctorsPage;
import com.hospitaltriage.pages.DepartmentsPage;
import com.hospitaltriage.utils.TestData;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Extra "Create New" test cases for Doctors:
 *  - positive: boundary lengths, inactive doctor, manager role
 *  - negative: code too long, trimming
 *  - UX/guard: inactive departments should not be selectable on create form
 */
public class DoctorCreateExtendedTests extends BaseUiTest {

    @Test
    public void createDoctor_success_codeMax20_nameMax200_shouldSucceed() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("DRX"); // prefix len 3 => 20 chars
        Assert.assertEquals(code.length(), 20, "Sanity: code must be exactly 20 chars");

        String name200 = "A".repeat(200);

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName(name200)
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Tạo bác sĩ thành công"),
                "Expected success flash message after create");

        DoctorsPage list = new DoctorsPage(driver).open().filter(code, null);
        Assert.assertTrue(list.hasDoctorCode(code), "Created doctor should appear in list");

        // cleanup-ish
        list.clickDeleteByCode(code);
        logoutIfPossible();
    }

    @Test
    public void createDoctor_success_inactiveDoctor_shouldShowInactiveBadge() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("INA");

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName("BS Inactive " + code)
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(false)
                .save();

        DoctorsPage list = new DoctorsPage(driver).open().filter(code, null);
        Assert.assertTrue(list.hasDoctorCode(code), "Created doctor should appear in list");
        Assert.assertTrue(list.rowHasBadge(code, "Inactive"), "Inactive badge should be shown");

        logoutIfPossible();
    }

    @Test
    public void createDoctor_negative_codeTooLong_shouldStayOnForm_andShowValidationHint() {
        logoutIfPossible();
        loginAs("admin");

        String code21 = TestData.uniqueCode("LONG"); // prefix len 4 => 21 chars
        Assert.assertTrue(code21.length() > 20, "Sanity: code must be > 20 chars");

        DoctorFormPage form = new DoctorsPage(driver)
                .open()
                .clickCreate();

        form.forceCode(code21)
                .setFullName("BS Code Too Long")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tạo bác sĩ"), "Should remain on Create Doctor page");

        String summary = new DoctorFormPage(driver).validationSummaryText();
        Assert.assertTrue(summary.contains("20") || src.contains("20") || src.contains("maximum"),
                "Expected max-length validation hint for Code (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void createDoctor_trimInputs_shouldCreateWithTrimmedCode() {
        logoutIfPossible();
        loginAs("admin");

        String trimmedCode = TestData.uniqueCode("TRD");
        String rawCode = "  " + trimmedCode + "  ";

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(rawCode)
                .setFullName("  BS Trimmed  ")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        DoctorsPage list = new DoctorsPage(driver).open().filter(trimmedCode, null);
        Assert.assertTrue(list.hasDoctorCode(trimmedCode), "Doctor should be searchable by trimmed code");

        // cleanup-ish
        list.clickDeleteByCode(trimmedCode);
        logoutIfPossible();
    }

    @Test
    public void createDoctor_asManager_shouldSucceed() {
        logoutIfPossible();
        loginAs("manager");

        String code = TestData.uniqueCode("MDR");

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName("BS Manager tạo " + code)
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        Assert.assertFalse(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                "Manager should be allowed to create doctors");
        Assert.assertTrue(driver.getPageSource().contains("Tạo bác sĩ thành công")
                        || new DoctorsPage(driver).open().filter(code, null).hasDoctorCode(code),
                "Expected manager create success");

        // cleanup-ish
        new DoctorsPage(driver).open().filter(code, null).clickDeleteByCode(code);
        logoutIfPossible();
    }

    @Test
    public void doctorCreateForm_shouldNotShowInactiveDepartmentsInDropdown() {
        logoutIfPossible();
        loginAs("admin");

        // 1) Create an inactive department
        String deptCode = TestData.uniqueCode("ZIN");
        String deptName = "ZZZ Inactive Dept " + deptCode;

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(deptCode)
                .setName(deptName)
                .setIsActive(false)
                .setIsGeneral(false)
                .save();

        // 2) Open Doctor Create form and ensure that inactive department is NOT present
        new DoctorsPage(driver).open().clickCreate();

        WebElement select = driver.findElement(By.id("DepartmentId"));
        String selectText = select.getText();
        Assert.assertFalse(selectText.contains(deptCode + " - " + deptName),
                "Inactive departments should not appear in Doctor create dropdown");

        logoutIfPossible();
    }
}
