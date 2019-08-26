from pydicom.dataset import Dataset

from pynetdicom import AE
from pynetdicom.sop_class import PatientRootQueryRetrieveInformationModelFind
from pynetdicom.sop_class import StudyRootQueryRetrieveInformationModelFind
from pynetdicom.sop_class import PatientStudyOnlyQueryRetrieveInformationModelFind
from pydicom._dicom_dict import DicomDictionary
from flask import Flask, jsonify, make_response
import json

app = Flask(__name__)

IPLIST = {'' : '', '': ''}
PORT = 104
LOCAL_PORT = 5051
AE_TITLE = 'VT'

def get_basedataset():
     ds = Dataset()
     ds.PatientName = ''
     ds.PatientID = ''
     ds.StudyInstanceUID = ''
     ds.SeriesUID=''
     ds.AccessionNumber = ''
     ds.QueryRetrieveLevel = ''
     ds.PatientSize = ''
     ds.ModalitiesInStudy = ''
     ds.SOPClassesInStudy = ''
     ds.StudyDescription = '*'
     ds.SeriesDescription=''
     ds.FailedSOPSequence = ''
     ds.DerivationCodeSequence =''
     ds.PatientSpeciesCodeSequence =''
     ds.Series = ''
     ds.TransferSyntaxUID =''
     ds.SeriesInStudy=''
     ds.SeriesNumber=''
     ds.ImagesInSeries=''
     ds.SeriesInstanceUID=''
     ds.NumberOfSeriesRelatedInstances=''
     ds.StudyDate=''

     return ds

def make_ae_req(aet, ds):
     ae = AE(AE_TITLE)
     ae.add_requested_context(StudyRootQueryRetrieveInformationModelFind)
     ae.add_requested_context(PatientRootQueryRetrieveInformationModelFind)
     
     # Associate with peer AE 
     assoc = ae.associate(IPLIST[aet], PORT, ae_title=aet)

     if assoc.is_established:
          # Use the C-FIND service to send the identifier
          # A query_model value of 'P' means use the 'Patient Root Query Retrieve
          #     Information Model - Find' presentation context
          responses = assoc.send_c_find(ds, query_model='S')
          result = []
          for (status, identifier) in responses:
              if status:
                  # If the status is 'Pending' then identifier is the C-FIND response
                  if status.Status in (0xFF00, 0xFF01):
                      result.append(str(identifier))
              else:
                  return 'Connection timed out, was aborted or received invalid response'

          # Release the association
          assoc.release()
          resp = make_response(json.dumps(result), 200)
          resp.headers['content-type'] = 'text/json'
          return resp
     else:
          return 'Association rejected, aborted or never connected'

@app.route('/<string:aet>/series/bystudyuid/<string:study_uid>', methods=['GET'])
def find_series_studyuid(aet, study_uid):
     ds = get_basedataset()
     ds.StudyInstanceUID = study_uid
     ds.QueryRetrieveLevel = 'SERIES'
     return make_ae_req(aet, ds)

@app.route('/<string:aet>/study/bypatient/<string:patient_id>', methods=['GET'])
def find_study_patient(aet, patient_id):
     ds = get_basedataset()
     ds.PatientID = patient_id
     ds.QueryRetrieveLevel = 'STUDY'
     return make_ae_req(aet, ds)

@app.route('/<string:aet>/study/byaccession/<string:accession_id>', methods=['GET'])
def find_study_accession(aet, accession_id):
     ds = get_basedataset()
     ds.AccessionNumber = accession_id
     ds.QueryRetrieveLevel = 'STUDY'
     return make_ae_req(aet, ds)


if __name__ == '__main__':
    app.run(port=LOCAL_PORT, debug=True)





