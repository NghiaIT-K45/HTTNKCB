package com.hospitaltriage.tests;

import com.hospitaltriage.pages.DashboardPage;
import com.hospitaltriage.pages.IntakePage;
import com.hospitaltriage.pages.TriageListPage;
import com.hospitaltriage.pages.TriageProcessPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

public class VisitStatusFlowTests extends BaseUiTest {

    private void createVisitAndTriageToNoi(String patientName, String identityNumber) {
        // Intake as Receptionist
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(35));
        new IntakePage(driver).open()
                .setFullName(patientName)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(identityNumber)
                .setPhone(TestData.phone())
                .setInsuranceCode("BHYT" + identityNumber)
                .setAddress("TP.HCM")
                .submit();

        // Triage as Nurse, rely on rule engine keyword "ho" -> Khoa Nội
        logoutIfPossible();
        loginAs("nurse");

        String today = TestData.date(LocalDate.now());
        TriageProcessPage process = new TriageListPage(driver)
                .open()
                .filterDate(today)
                .clickProcessByPatientName(patientName);

        process.setSymptoms("ho nhẹ")
                .submit();
    }

    @Test
    public void doctor_can_startExamination_fromDashboard_waitingDoctorRowDisappears() {
        String patientName = "BN FLOW " + TestData.uniqueCode("");
        String idNo = TestData.identityNumber();

        createVisitAndTriageToNoi(patientName, idNo);

        logoutIfPossible();
        loginAs("doctor1");

        DashboardPage dash = new DashboardPage(driver).open();
        Assert.assertTrue(dash.hasPatient(patientName),
                "Doctor should see the triaged visit in dashboard waiting list");

        dash.startExaminationForPatient(patientName);

        Assert.assertTrue(driver.getPageSource().contains("Cập nhật trạng thái"),
                "Should show status update message after starting examination");

        Assert.assertFalse(dash.hasPatient(patientName),
                "After starting examination, the visit should not remain in waiting list (status is no longer WaitingDoctor)");

        logoutIfPossible();
    }
}
