/*

When Course grade level offered are within grades 9-12, If N is selected, then NOT X

*/

DECLARE @RuleId VARCHAR(32) = '50.02.0007';
DECLARE @Message NVARCHAR(MAX) = 'Course level type combination error detected for course offered for grades 9-12. if course level type N is selected for the course, X cannot also be selected on the same course.';
DECLARE @IsError BIT = 1;

WITH 
course_level_types AS (

SELECT
 CourseLevelType.EducationOrganizationId,
 CourseLevelType.CourseCode,
 CourseLevel.CodeValue AS CourseLevelCodeValue
FROM 
 mn.CourseLevelType
 INNER JOIN 
 edfi.Descriptor CourseLevel
  ON CourseLevelTypeDescriptorId = CourseLevel.DescriptorId
 INNER JOIN
 edfi.CourseOfferedGradeLevel
  ON CourseOfferedGradeLevel.EducationOrganizationId = CourseLevelType.EducationOrganizationId
   AND CourseOfferedGradeLevel.CourseCode = CourseLevelType.CourseCode
 INNER JOIN
 edfi.GradeLevelDescriptor
  ON GradeLevelDescriptor.GradeLevelDescriptorId = CourseOfferedGradeLevel.GradeLevelDescriptorId
 INNER JOIN
 edfi.Descriptor GradeLevel
  ON GradeLevel.DescriptorId = GradeLevelDescriptor.GradeLevelDescriptorId
WHERE
 CourseLevelType.EducationOrganizationId = @DistrictId
 AND GradeLevel.CodeValue IN ('9','10','11','12')
GROUP BY
 CourseLevelType.EducationOrganizationId,
 CourseLevelType.CourseCode,
 CourseLevel.CodeValue

 ),
failed_rows AS (

SELECT
 DISTINCT CourseCode
FROM
 course_level_types
WHERE
 CourseLevelCodeValue = 'N'
 AND EXISTS (
  SELECT 1
  FROM
   course_level_types inside
  WHERE
   inside.EducationOrganizationId = course_level_types.EducationOrganizationId
   AND inside.CourseCode = course_level_types.CourseCode
   AND inside.CourseLevelCodeValue = 'X'
   )

)
INSERT INTO 
 rules.RuleValidationDetail (RuleValidationId, Id, RuleId, IsError, [Message])
SELECT TOP 1
 @RuleValidationId, @DistrictId, @RuleId RuleId, @IsError IsError, 
 @Message + (
  SELECT TOP 1 
   ' Course Code: '+CourseCode
  FROM failed_rows
  ) [Message]
FROM failed_rows;
