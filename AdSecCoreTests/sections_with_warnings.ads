{
    "filePath": "D:\\REPOS\\AdSec-GH\\AdSecCoreTests\\sections_with_warnings.ads",
    "writtenBy": {
        "ProgramName": "AdSec",
        "Company": "Oasys Ltd.",
        "Copyright": "Copyright © Oasys 1985-2025",
        "Description": "AdSec 10",
        "ProgramVersion": "10.0 build 16",
        "FullVersion": "10.0.16.53"
    },
    "titles": {},
    "modelId": {
        "modelId": "8bb59429-603e-465b-8235-2b8839c60e0d",
        "parentModelId": "a96a9574-0c3d-46a8-996f-aa8829ad1657",
        "rootModelId": "bc0bd339-a15c-4f71-9a57-b151eb1ab990"
    },
    "units": {
        "force": "N",
        "length": "m",
        "sectionDims": "m",
        "stress": "Pa",
        "mass": "kg",
        "strain": "ε",
        "temperature": "°C"
    },
    "codes": {
        "concrete": "EC2_GB_04"
    },
    "materials": {
        "concrete": [
            {
                "name": "C12/15",
                "strength": 12000000.0,
                "elasticModulus": 27085177093.588159,
                "density": 2400.0,
                "coefficientOfThermalExpansion": 0.00001,
                "poissonsRatio": 0.2,
                "ULS": {
                    "gammaF": 1.5,
                    "gammaE": 1.0,
                    "tension": {
                        "model": "NO_TENSION",
                        "failureStrain": 1.0
                    },
                    "compression": {
                        "model": "RECT_PARABOLA",
                        "plasticStrainLimit": 0.002,
                        "failureStrain": 0.0035
                    }
                },
                "SLS": {
                    "gammaF": 1.0,
                    "gammaE": 1.0,
                    "tension": {
                        "model": "INTERPOLATED",
                        "yieldStrain": 0.0,
                        "plasticStrainLimit": 0.0,
                        "failureStrain": 0.00005805554939116794
                    },
                    "compression": {
                        "model": "FIB_SCHEMATIC",
                        "plasticStrainLimit": 0.001771810775885091,
                        "failureStrain": 0.0035
                    }
                },
                "type": "Normal weight concrete",
                "confinedStrength": 0.0,
                "materialType": "Concrete"
            }
        ],
        "reinforcement": [
            {
                "name": "500B",
                "strength": 500000000.0,
                "elasticModulus": 200000000000.0,
                "density": 7850.0,
                "coefficientOfThermalExpansion": 0.000012,
                "poissonsRatio": 0.3,
                "ULS": {
                    "gammaF": 1.15,
                    "gammaE": 1.0,
                    "tension": {
                        "model": "ELAS_HARD",
                        "yieldStrain": 0.002173913043478261,
                        "failureStrain": 0.045000000000000008
                    },
                    "compression": {
                        "model": "ELAS_HARD",
                        "yieldStrain": 0.002173913043478261,
                        "failureStrain": 0.045000000000000008
                    }
                },
                "SLS": {
                    "gammaF": 1.0,
                    "gammaE": 1.0,
                    "tension": {
                        "model": "ELAS_HARD",
                        "yieldStrain": 0.0025,
                        "failureStrain": 0.045000000000000008
                    },
                    "compression": {
                        "model": "ELAS_HARD",
                        "yieldStrain": 0.0025,
                        "failureStrain": 0.045000000000000008
                    }
                },
                "type": "Steel rebar",
                "label": "B",
                "ultimateStrain": 0.05,
                "hardeningModulus": 727272727.272728,
                "hardeningParameter": 1.08,
                "ductility": "NORMAL"
            }
        ]
    },
    "sections": [
        {
            "components": [
                {
                    "material": "concrete",
                    "grade": "C12/15",
                    "profile": "STD R(m) 0.6 0.3",
                    "reinforcement": {
                        "cover": 0.03,
                        "positionsRelativeTo": "ORIGIN",
                        "groups": [
                            {
                                "type": "LINK",
                                "position": "",
                                "description": "\"500B\"0.01",
                                "preload": {
                                    "preloadType": "NONE",
                                    "value": 0.0,
                                    "exclude": true
                                }
                            },
                            {
                                "type": "TOP",
                                "position": "",
                                "description": "4\"500B\"0.016",
                                "preload": {
                                    "preloadType": "FORCE",
                                    "value": 0.0,
                                    "exclude": true
                                }
                            },
                            {
                                "type": "BOTTOM",
                                "position": "",
                                "description": "4\"500B\"0.025",
                                "preload": {
                                    "preloadType": "FORCE",
                                    "value": 0.0,
                                    "exclude": true
                                }
                            },
                            {
                                "type": "SIDES",
                                "position": "",
                                "description": "\"500B\"0.016-0.2",
                                "preload": {
                                    "preloadType": "FORCE",
                                    "value": 0.0,
                                    "exclude": true
                                }
                            }
                        ]
                    }
                }
            ],
            "tasks": [
                {
                    "action": "LOAD",
                    "state": "ULS",
                    "preloadInclCurv": false,
                    "outputOptions": {
                        "uls": {
                            "status": true,
                            "utilisation": true,
                            "momentRatio": false,
                            "response": false,
                            "reductionFactor": false
                        },
                        "sls": {
                            "cracked": false,
                            "crackWidth": false,
                            "stiffness": false
                        }
                    },
                    "codeOptions": {
                        "crackCalc": "LOCAL",
                        "Cnom": 0.0,
                        "userDefinedPhiLower": 0.0,
                        "userDefinedPhiHigher": 0.0,
                        "userDefinedStrainLower": 0.0,
                        "userDefinedStrainHigher": 0.0
                    },
                    "loadTerm": "SHORT",
                    "componentActiveStates": [
                        {
                            "componentID": 1,
                            "activeState": true
                        }
                    ]
                }
            ],
            "extents": {
                "yMin": -0.15,
                "yMax": 0.15,
                "zMin": 0.3,
                "zMax": -0.3
            },
            "properties": [
                {
                    "analysis": {
                        "area": 0.18,
                        "localAxis": {
                            "iyy": 0.005399999999999999,
                            "izz": 0.00135,
                            "iyz": -0.0
                        },
                        "principalAxis": {
                            "iuu": 0.005399999999999999,
                            "ivv": 0.0013499999999999997,
                            "angle": 0.0
                        },
                        "shear": {
                            "ky": 0.8333333,
                            "kz": 0.8333333
                        },
                        "torsion": {
                            "j": 0.0037078593209999989
                        },
                        "elastic": {
                            "zy": 0.018,
                            "zz": 0.009000000000000001
                        },
                        "plastic": {
                            "zpy": 0.027,
                            "zpz": 0.0135
                        },
                        "centroid": {
                            "cy": 0.0,
                            "cz": 0.0
                        },
                        "radiusOfGyration": {
                            "ry": 0.17320508075688774,
                            "rz": 0.08660254037844387
                        },
                        "physical": {
                            "surfaceArea": 1.8
                        }
                    },
                    "paths": [
                        {
                            "isVoid": false,
                            "points": [
                                {
                                    "y": -0.15,
                                    "z": 0.3
                                },
                                {
                                    "y": -0.15,
                                    "z": -0.3
                                },
                                {
                                    "y": 0.15,
                                    "z": -0.3
                                },
                                {
                                    "y": 0.15,
                                    "z": 0.3
                                }
                            ]
                        }
                    ],
                    "links": [
                        {
                            "grade": 1,
                            "diameter": 0.01,
                            "path": "M -0.115000 -0.240000 A 0.025000 0.025000 0 0 1 -0.090000 -0.265000 L 0.090000 -0.265000 A 0.025000 0.025000 0 0 1 0.115000 -0.240000 L 0.115000 0.240000 A 0.025000 0.025000 0 0 1 0.090000 0.265000 L -0.090000 0.265000 A 0.025000 0.025000 0 0 1 -0.115000 0.240000 L -0.115000 -0.240000"
                        }
                    ],
                    "bars": [
                        {
                            "groupId": 1,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": -0.09848528137423855,
                            "z": 0.2484852813742386
                        },
                        {
                            "groupId": 1,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": -0.034,
                            "z": 0.252
                        },
                        {
                            "groupId": 1,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": 0.033999999999999978,
                            "z": 0.252
                        },
                        {
                            "groupId": 1,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": 0.09848528137423855,
                            "z": 0.2484852813742386
                        },
                        {
                            "groupId": 2,
                            "grade": 1,
                            "diameter": 0.025,
                            "y": -0.0953033008588991,
                            "z": -0.24530330085889913
                        },
                        {
                            "groupId": 2,
                            "grade": 1,
                            "diameter": 0.025,
                            "y": -0.0325,
                            "z": -0.2475
                        },
                        {
                            "groupId": 2,
                            "grade": 1,
                            "diameter": 0.025,
                            "y": 0.03249999999999999,
                            "z": -0.2475
                        },
                        {
                            "groupId": 2,
                            "grade": 1,
                            "diameter": 0.025,
                            "y": 0.0953033008588991,
                            "z": -0.24530330085889913
                        },
                        {
                            "groupId": 3,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": -0.10199999999999998,
                            "z": 0.08666666666666667
                        },
                        {
                            "groupId": 3,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": -0.10199999999999998,
                            "z": -0.08666666666666667
                        },
                        {
                            "groupId": 3,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": 0.10199999999999998,
                            "z": -0.08666666666666667
                        },
                        {
                            "groupId": 3,
                            "grade": 1,
                            "diameter": 0.016,
                            "y": 0.10199999999999998,
                            "z": 0.08666666666666667
                        }
                    ]
                }
            ]
        }
    ]
}
