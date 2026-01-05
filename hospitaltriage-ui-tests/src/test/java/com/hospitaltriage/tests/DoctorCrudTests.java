package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DoctorsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

public class DoctorCrudTests extends BaseUiTest {

    private static final String NEW_DOCTOR_CODE = TestData.uniqueCode("DRQA");
    private static final String NEW_DOCTOR_NAME = "BS QA Automation";

    @Test
    public void createDoctor_requiredValidation_codeAndName() {
        logoutIfPossible();
        loginAs("admin");

        var form = new DoctorsPage(driver).open().clickCreate();
        form.setCode("")
                .setFullName("")
                // do not select department
                .setIsActive(true)
                .save();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Code là bắt buộc") || src.contains("Mã bác sĩ"), "Missing code validation not found");
        Assert.assertTrue(src.contains("Họ tên") || src.contains("bắt buộc"), "Missing name validation not found");
    }

    @Test
    public void createDoctor_duplicateCodeValidation() {
        logoutIfPossible();
        loginAs("admin");

        var form = new DoctorsPage(driver).open().clickCreate();
        form.setCode("DR001")
                .setFullName("Dup DR001")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Mã bác sĩ đã tồn tại"),
                "Should show duplicate doctor code error");
    }

    @Test(dependsOnMethods = "createDoctor_duplicateCodeValidation")
    public void createDoctor_success_then_editDepartment() {
        logoutIfPossible();
        loginAs("admin");

        var list = new DoctorsPage(driver).open();
        var form = list.clickCreate();
        form.setCode(NEW_DOCTOR_CODE)
                .setFullName(NEW_DOCTOR_NAME)
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        list.open().filter(NEW_DOCTOR_CODE, null);
        Assert.assertTrue(list.hasDoctorCode(NEW_DOCTOR_CODE), "Doctor should appear after create");
        Assert.assertTrue(list.departmentNameInRow(NEW_DOCTOR_CODE).contains("Khoa Nội"), "Expected department Nội");

        // edit department
        var edit = list.clickEditByCode(NEW_DOCTOR_CODE);
        edit.selectDepartmentByVisibleText("NGOAI - Khoa Ngoại").save();

        list.open().filter(NEW_DOCTOR_CODE, null);
        Assert.assertTrue(list.departmentNameInRow(NEW_DOCTOR_CODE).contains("Khoa Ngoại"), "Expected department Ngoại after edit");

        logoutIfPossible();
    }

    @Test(dependsOnMethods = "createDoctor_success_then_editDepartment")
    public void deleteDoctor_deactivate() {
        logoutIfPossible();
        loginAs("admin");

        var list = new DoctorsPage(driver).open().filter(NEW_DOCTOR_CODE, null);
        Assert.assertTrue(list.hasDoctorCode(NEW_DOCTOR_CODE), "Doctor must exist before delete");

        list.clickDeleteByCode(NEW_DOCTOR_CODE).filter(NEW_DOCTOR_CODE, null);
        // After deactivate, row still exists but is Inactive badge; easiest assert by page source
        Assert.assertTrue(driver.getPageSource().contains("Inactive"), "Inactive badge should appear after deactivate");

        logoutIfPossible();
    }
}
