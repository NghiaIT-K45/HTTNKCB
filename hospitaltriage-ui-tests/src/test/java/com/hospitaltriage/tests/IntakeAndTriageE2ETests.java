package com.hospitaltriage.tests;

import com.hospitaltriage.pages.*;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

public class IntakeAndTriageE2ETests extends BaseUiTest {

    private IntakeResultPage registerPatientAsReceptionist(String patientName, String identityNumber) {
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(25));

        var intake = new IntakePage(driver).open();
        return intake
                .setFullName(patientName)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(identityNumber)
                .setPhone(TestData.phone())
                .setInsuranceCode("BHYT" + identityNumber)
                .setAddress("Hà Nội")
                .submit();
    }

    @Test
    public void intake_upsert_existingPatient_secondTime_shouldBeExisting_andQueueIncreases() {
        logoutIfPossible();

        String name = TestData.vietnamesePatientName();
        String idNo = TestData.identityNumber();

        var r1 = registerPatientAsReceptionist(name, idNo);
        Assert.assertTrue(r1.isNewPatient(), "First intake should create new patient");
        int pId1 = r1.patientId();
        int q1 = r1.queueNumber();

        r1.startNewIntake();

        // second time with same IdentityNumber should update existing patient and create new visit
        var r2 = new IntakePage(driver)
                .setFullName(name)
                .setDateOfBirth(TestData.date(LocalDate.now().minusYears(25)))
                .selectGender("Nam")
                .setIdentityNumber(idNo)
                .setPhone(TestData.phone())
                .setInsuranceCode("BHYT" + idNo)
                .setAddress("Hà Nội")
                .submit();

        Assert.assertFalse(r2.isNewPatient(), "Second intake should reuse existing patient");
        Assert.assertEquals(r2.patientId(), pId1, "PatientId should be the same when upsert hits existing");

        int q2 = r2.queueNumber();
        Assert.assertTrue(q2 > q1, "QueueNumber should increase for subsequent visit in same day");

        logoutIfPossible();
    }

    @Test
    public void triage_ruleEngine_autoAssignDepartment_bySymptoms() {
        logoutIfPossible();

        // 1) create visit
        String patientName = TestData.vietnamesePatientName();
        String idNo = TestData.identityNumber();
        registerPatientAsReceptionist(patientName, idNo);

        logoutIfPossible();

        // 2) triage as Nurse
        loginAs("nurse");

        String today = TestData.date(LocalDate.now());
        var list = new TriageListPage(driver).open().filterDate(today);
        Assert.assertTrue(list.hasPatient(patientName), "Patient should appear in WaitingTriage list");

        var process = list.clickProcessByPatientName(patientName);

        // validation: symptoms required
        process.setSymptoms("").submit();
        Assert.assertTrue(driver.getPageSource().contains("Triệu chứng là bắt buộc"),
                "Should show required Symptoms validation");

        // now do proper triage (rule keywords seeded: sốt/ho/đau bụng -> Khoa Nội)
        process = new TriageListPage(driver).open().filterDate(today).clickProcessByPatientName(patientName);
        process.setSymptoms("SỐT nhẹ và HO nhiều")
                // do NOT select department/doctor => test auto-suggest + fallback logic
                .submit();

        // 3) verify dashboard
        var dash = new DashboardPage(driver);
        Assert.assertTrue(dash.hasPatient(patientName), "Patient should appear in Dashboard waiting list after triage");
        Assert.assertTrue(dash.departmentForPatient(patientName).contains("Khoa Nội"),
                "RuleEngine should assign Khoa Nội for symptoms containing 'sốt'/'ho'");

        logoutIfPossible();
    }

    @Test
    public void triage_fallbackToGeneralDepartment_whenNoRuleMatch() {
        logoutIfPossible();

        // 1) create visit
        String patientName = TestData.vietnamesePatientName();
        String idNo = TestData.identityNumber();
        registerPatientAsReceptionist(patientName, idNo);

        logoutIfPossible();

        // 2) triage as Nurse with no matching keywords
        loginAs("nurse");
        String today = TestData.date(LocalDate.now());

        var process = new TriageListPage(driver).open().filterDate(today).clickProcessByPatientName(patientName);
        process.setSymptoms("abcxyz (không match rule)")
                .submit();

        // 3) verify dashboard assigns General (GEN - Khoa Tổng Quát)
        var dash = new DashboardPage(driver);
        Assert.assertTrue(dash.hasPatient(patientName), "Patient should appear in Dashboard after triage");
        Assert.assertTrue(dash.departmentForPatient(patientName).contains("Khoa Tổng Quát"),
                "Should fallback to General department when no rule matches");

        logoutIfPossible();
    }

    @Test
    public void triage_manualOverrideDepartment_andSelectDoctor() {
        logoutIfPossible();

        String patientName = TestData.vietnamesePatientName();
        String idNo = TestData.identityNumber();
        registerPatientAsReceptionist(patientName, idNo);

        logoutIfPossible();

        loginAs("nurse");
        String today = TestData.date(LocalDate.now());

        var process = new TriageListPage(driver).open().filterDate(today).clickProcessByPatientName(patientName);
        process.setSymptoms("sốt (nhưng sẽ override sang Ngoại)")
                .selectDepartment("NGOAI - Khoa Ngoại")
                .selectDoctor("DR002 - BS Trần Thị B")
                .submit();

        var dash = new DashboardPage(driver);
        Assert.assertTrue(dash.hasPatient(patientName), "Patient should appear in Dashboard after triage");
        Assert.assertTrue(dash.departmentForPatient(patientName).contains("Khoa Ngoại"),
                "Manual department selection should override rule suggestion");
        Assert.assertTrue(dash.doctorForPatient(patientName).contains("Trần Thị B"),
                "Selected doctor should appear on dashboard");

        logoutIfPossible();
    }
}
