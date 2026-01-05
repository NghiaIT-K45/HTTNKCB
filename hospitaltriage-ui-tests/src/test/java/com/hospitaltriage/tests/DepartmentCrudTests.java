package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DepartmentsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

public class DepartmentCrudTests extends BaseUiTest {

    private static final String NEW_DEPT_CODE = TestData.uniqueCode("QA");
    private static final String NEW_DEPT_NAME = "Khoa QA Automation";

    @Test
    public void createDepartment_requiredFieldValidation() {
        logoutIfPossible();
        loginAs("admin");

        var form = new DepartmentsPage(driver).open().clickCreate();
        form.setCode("").setName(NEW_DEPT_NAME).setIsActive(true).setIsGeneral(false).save();
        Assert.assertTrue(driver.getPageSource().contains("Code là bắt buộc"),
                "Should show required Code validation");
    }

    @Test
    public void createDepartment_duplicateCodeValidation() {
        logoutIfPossible();
        loginAs("admin");

        var form = new DepartmentsPage(driver).open().clickCreate();
        form.setCode("GEN").setName("Dup GEN").setIsActive(true).setIsGeneral(false).save();
        Assert.assertTrue(driver.getPageSource().contains("Mã khoa đã tồn tại"),
                "Should show duplicate department code error");
    }

    @Test(dependsOnMethods = "createDepartment_duplicateCodeValidation")
    public void createDepartment_success_setGeneral_switchPreviousGeneral() {
        logoutIfPossible();
        loginAs("admin");

        var list = new DepartmentsPage(driver).open();
        var form = list.clickCreate();
        form.setCode(NEW_DEPT_CODE)
                .setName(NEW_DEPT_NAME)
                .setIsActive(true)
                .setIsGeneral(true)
                .save();

        // verify created
        list.open().search(NEW_DEPT_CODE);
        Assert.assertTrue(list.hasDepartmentCode(NEW_DEPT_CODE), "New department should appear in list");
        Assert.assertTrue(list.rowHasBadge(NEW_DEPT_CODE, "General"), "New dept should be General");

        // verify old GEN is no longer general
        list.open().search("GEN");
        Assert.assertFalse(list.rowHasBadge("GEN", "General"), "GEN should no longer be General after switch");

        logoutIfPossible();
    }

    @Test(dependsOnMethods = "createDepartment_success_setGeneral_switchPreviousGeneral")
    public void editDepartment_updateName_andUnsetGeneral() {
        logoutIfPossible();
        loginAs("admin");

        var list = new DepartmentsPage(driver).open().search(NEW_DEPT_CODE);
        var form = list.clickEditByCode(NEW_DEPT_CODE);
        form.setName(NEW_DEPT_NAME + " - Updated")
                .setIsGeneral(false)
                .save();

        // basic check: still exists
        list.open().search(NEW_DEPT_CODE);
        Assert.assertTrue(list.hasDepartmentCode(NEW_DEPT_CODE), "Department should still exist");

        // IMPORTANT: restore the seeded General department to keep triage fallback tests stable
        // (rule engine fallback relies on a General department existing).
        list.open().search("GEN").clickEditByCode("GEN").setIsGeneral(true).save();

        logoutIfPossible();
    }

    @Test(dependsOnMethods = "editDepartment_updateName_andUnsetGeneral")
    public void deleteDepartment_deactivate() {
        logoutIfPossible();
        loginAs("admin");

        var list = new DepartmentsPage(driver).open().search(NEW_DEPT_CODE);
        Assert.assertTrue(list.hasDepartmentCode(NEW_DEPT_CODE), "Department must exist before delete");

        list.clickDeleteByCode(NEW_DEPT_CODE).search(NEW_DEPT_CODE);
        Assert.assertTrue(list.rowHasBadge(NEW_DEPT_CODE, "Inactive"), "Department should be deactivated");

        logoutIfPossible();
    }
}
